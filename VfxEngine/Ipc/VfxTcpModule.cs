//************************************************************************
// The VersaFix FIX Engine. 
// Copyright (c) 2009 Assimilate Technology, Inc.
//------------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxTcpModule provides an implementation of an IPC module
    /// that performs asynchronous I/O over a socket connection.
    /// </summary>
    public class VfxTcpModule : IVfxIpcModule
    {
        private System.Guid Id = System.Guid.NewGuid();

        /// <summary>
        /// The TxContext class is an internal class that is used
        /// for tracking the state of an asynchronous send:
        /// </summary>
        private class IoContext
        {
            // REC: The socket used for the operation:
            public Socket _ipcSocket;
            // REC: The buffer used for the operation:
            public VfxMsgBlock _ipcBuffer;

            public IoContext(Socket socket, VfxMsgBlock buffer)
            {
                _ipcSocket = socket;
                _ipcBuffer = buffer;
            }
        }

        private enum ModuleStates
        {
            Module_State_Active,
            Module_State_Closing,
            Module_State_Shutdown
        }

        // REC: The socket that is being communicated through:
        private Socket _socket;

        // REC: The flag that indicates whether or not the module
        // should buffer incoming data onto a secondary thread:
        private bool _rxBuffering = false;

        // REC: The number of transmit context entries that are 
        // to be created in advance for asynchronous IO:
        private int _maxTxContexts = 256;

        // REC: The number of receive context instances that will
        // be allocated in advance for asynchronous IO:
        private int _maxRxContexts = 256;

        // REC: The queue for unallocated transmit contexts:
        private Queue<IoContext> _ioContextQueue = new Queue<IoContext>();

        // REC: Synchronization object for the buffers queue:
        private object _synchIoContextQueue = new object();

        // REC: The queue for pending transmit contexts:
        private Queue<IoContext> _txContextQueue = new Queue<IoContext>();

        // REC: The queue for available receive contexts:
        private Queue<IoContext> _rxContextQueue = new Queue<IoContext>();

        // REC: Synchronization object for the receive context queue:
        private object _synchRxContextQueue = new object();

        // REC: The queue for processing received messages:
        private Queue<IoContext> _rxPendingQueue = new Queue<IoContext>();

        // REC: Synchronization object for the receive queue:
        private object _synchRxPendingQueue = new object();

        // REC: Synchronization object for the pending queue:
        private object _synchTxContextQueue = new object();

        // REC: Event that indicates the buffer queue is ready:
        private AutoResetEvent _eventTxBuffer = new AutoResetEvent(false);

        // REC: The event that indicates the receive buffer has
        // been populated with data and can be read from:
        private AutoResetEvent _eventRxBuffer = new AutoResetEvent(false);

        // REC: Flag that indicates a send is in progress:
        private bool _txPending = false;
        
        // REC: The current state that the module is in:
        private ModuleStates _moduleState = ModuleStates.Module_State_Shutdown;

        // REC: If the module's owner has configured the module
        // for buffered receive operation, then the system offloads
        // incoming data onto a separate queue, where it is processed
        // by a separate thread that dispatches the received data out
        // to the module's subscribers.
        private System.Threading.Thread _rxThread;

        /// <summary>
        /// The Init method is called to initialize the IPC module
        /// with an instance of a .NET socket that will be used to
        /// communicate with a peer system.
        /// </summary>
        /// <param name="socket">
        /// The socket that the module will use for communicating
        /// with the peer system.
        /// </param>
        /// <param name="buffered">
        /// The flag that indicates whether or not the module will
        /// offload incoming data onto a separate thread before it
        /// routes those messages up to the module's owner.
        /// </param>
        public void Init(Socket socket, bool buffered=false)
        {
            // REC: Retain a reference to the socket for use
            // with the send and receive operations:
            this._socket = socket;

            // REC: Configure the receive buffering flag:
            this._rxBuffering = buffered;

            // REC: Pre-allocate the transmit contexts that will
            // be used for sending data to the peer system:
            for (int i = 0; i != _maxTxContexts; i++)
            {
                IoContext txContext = new IoContext(socket, new VfxMsgBlock(8192));
                _ioContextQueue.Enqueue(txContext);
            }

            // REC: If the buffered flag is set, then the module will
            // push incoming data onto a separate thread for subsequent
            // processing by the owner of the module.
            if (buffered == true)
            {
                // REC: Allocate an initial bunch of IO contexts that
                // will be used for buffering incoming data. These are
                // going to be reused, so they are allocated up front:
                for (int i = 0; i != _maxRxContexts; i++)
                {
                    IoContext rxContext = new IoContext(socket, new VfxMsgBlock(8192));
                    _rxContextQueue.Enqueue(rxContext);
                }

                // REC: The module starts a secondary thread which will
                // be used to buffer incoming data as it is received from
                // the peer system that the module is communicating with:
                this._rxThread = new Thread(HandleReceive_Entrypoint);
                this._rxThread.Start();
            }
        }

        #region IVfxIpcModule Members

        /// <summary>
        /// The EventDispatch event is a multiplexing event that is
        /// used by the module to report various events to its owner.
        /// </summary>
        public event EventHandler<VfxIpcEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method is invoked to instruct the IPC module
        /// to start asynchronously processing IO.
        /// </summary>
        public void Activate()
        {
            if (_moduleState == ModuleStates.Module_State_Shutdown)
            {
                _moduleState = ModuleStates.Module_State_Active;

                // REC: Disable the Nagle algorithm:
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                // REC: Dispatch the Event_Opened event to the
                // module's owner:
                EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                if (tmp != null)
                {
                    tmp(this, new VfxIpcEventArgs(VfxIpcEventTypes.Event_Session_Opened));
                }

                // REC: Initiate the first receive operation against the
                // socket associated with the module. As receive operations
                // complete, the completion handler will take care of all the
                // subsequent IO context generation / management:
                IoContext rxContext = new IoContext(_socket, new VfxMsgBlock(8192));
                InitiateRecv(rxContext);
            }
        }
        
        /// <summary>
        /// The Shutdown method is invoked to instruct the module
        /// to terminate the connection to the peer system. If the
        /// graceful flag is set to true, then the module will wait
        /// until all pending I/O operations are completed before 
        /// </summary>
        /// <param name="graceful"></param>
        public void Shutdown(bool graceful)
        {
            if (_moduleState == ModuleStates.Module_State_Active)
            {
                _moduleState = ModuleStates.Module_State_Closing;

                if (graceful == false)
                {
                    // REC: Remove all pending send operations
                    // from the queue and return their buffers 
                    // to the buffer pool:
                    lock (_synchTxContextQueue)
                    {
                        while (_txContextQueue.Count > 0)
                        {
                            IoContext txContext = _txContextQueue.Dequeue();
                            txContext._ipcBuffer.RdIndex = 0;
                            txContext._ipcBuffer.WrIndex = 0;

                            lock (_synchIoContextQueue)
                            {
                                _ioContextQueue.Enqueue(txContext);
                            }
                        }
                    }

                    // REC: Close the socket that is being
                    // used to send data to the client:
                    _socket.Close();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// The HandleTx method is invoked to request that an instance
        /// of the VfxTcpModule send the data in the specified message
        /// block out to the peer system it is connected to.
        /// </summary>
        /// <param name="mb">
        /// The message block that contains the data that is to be sent
        /// to the peer system the module is connected to.
        /// </param>
        public void HandleTx(VfxMsgBlock mb)
        {
            if (_moduleState == ModuleStates.Module_State_Active)
            {
                // REC: Send all of the data in the buffer:
                while (mb.Length() > 0)
                {
                    IoContext ctx = null;
                    lock (_synchIoContextQueue)
                    {
                        if (_ioContextQueue.Count > 0)
                        {
                            ctx = _ioContextQueue.Dequeue();
                        }
                    }

                    if (ctx != null)
                    {
                        if (ctx._ipcBuffer.Remaining() >= mb.Length())
                        {
                            // REC: Append the contents of the buffer
                            // to the IO context's buffer and push it
                            // onto the outgoing queue:
                            ctx._ipcBuffer.UnsafeAppend(mb);
                            mb.RdIndex = mb.WrIndex;

                            InitiateSend(ctx, false);
                        }
                        else
                        {
                            // REC: Copy as much data as possible into
                            // the context and attempt to send that:
                            int txLength = ctx._ipcBuffer.Remaining();
                            ctx._ipcBuffer.UnsafeAppend(mb, txLength);

                            // REC: Adjust the source buffer's index:
                            mb.RdIndex += txLength;

                            InitiateSend(ctx, false);
                        }
                    }
                    else
                    {
                        // REC: Wait for a buffer to become available:
                        _eventTxBuffer.WaitOne();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// The InitiateSend method starts an asynchronous send
        /// operation using the supplied IO context. If there is
        /// already a send operation in progress, the IO context
        /// will be enqueued to the pending IO queue and will be
        /// sent when the current operation completes.
        /// </summary>
        /// <param name="ctx">
        /// The IO context that contains the buffer to be sent.
        /// </param>
        /// <param name="internalSend">
        /// Flag that indicates whether or not the send operation
        /// is being initiated from internal or external code.
        /// </param>
        private void InitiateSend(IoContext ctx, bool internalSend)
        {
            if (internalSend == true)
            {
                ctx._ipcSocket.BeginSend(ctx._ipcBuffer.Buffer, ctx._ipcBuffer.RdIndex,
                            ctx._ipcBuffer.Length(), 0, CompleteSend, ctx);
            }
            else
            {
                //lock (_txContextQueue)
                lock(_synchTxContextQueue)
                {
                    if (_txPending == false)
                    {
                        // REC: The pending flag gets set so that the
                        // next call to InitiateSend can determine if
                        // it can send or needs to queue the request:
                        _txPending = true;
                        ctx._ipcSocket.BeginSend(ctx._ipcBuffer.Buffer, ctx._ipcBuffer.RdIndex,
                            ctx._ipcBuffer.Length(), 0, CompleteSend, ctx);
                        
                    }
                    else
                    {
                        // REC: If a send is already pending, then the
                        // request is queued for the completion handler
                        // to pick up when the current send is done:
                        _txContextQueue.Enqueue(ctx);
                    }
                }
            }
        }

        /// <summary>
        /// The CompleteSend event handler is invoked in response to an
        /// asynchronous send operation being completed. The system reacts
        /// to the completion by finalizing the asynchronous operation and
        /// then starting a new one.
        /// </summary>
        /// <param name="ar"></param>
        private void CompleteSend(IAsyncResult ar)
        {
            IoContext ctx = ar.AsyncState as IoContext;
            if (ctx != null)
            {
                int txBytes = ctx._ipcSocket.EndSend(ar);
                if (txBytes > 0)
                {
                    // REC: If the system did not transmit all of the data
                    // that was in the IO context buffer then it initiates
                    // a new send operation against the residual content:
                    if (txBytes < ctx._ipcBuffer.Length())
                    {
                        // REC: Adjust the read index...
                        ctx._ipcBuffer.RdIndex += txBytes;
                        ctx._ipcBuffer.Crunch();

                        // REC: and try to send it again...
                        InitiateSend(ctx, true);
                    }
                    else
                    {
                        lock (_synchIoContextQueue)
                        {
                            // REC: Reset the message block's
                            // read and write indices:
                            ctx._ipcBuffer.RdIndex = 0;
                            ctx._ipcBuffer.WrIndex = 0;

                            // REC: Return the buffer to the
                            // pool so it can be used again:
                            _ioContextQueue.Enqueue(ctx);

                            // REC: If there were no contexts remaining
                            // in the transmit queue when this one was 
                            // returned to it, trigger the buffer event
                            // so that HandleTx can acquire one:
                            if (_ioContextQueue.Count == 1)
                            {
                                _eventTxBuffer.Set();
                            }
                        }

                        lock (_synchTxContextQueue)
                        {
                            if (_txContextQueue.Count > 0)
                            {
                                IoContext next = _txContextQueue.Dequeue();
                                InitiateSend(next, true);
                            }
                            else
                            {
                                _txPending = false;

                                // REC: If the module is being shutdown
                                // and there are no further pending sends
                                // left to process, generate the shutdown
                                // event to notify the module's owner:
                                if (_moduleState == ModuleStates.Module_State_Closing)
                                {
                                    _moduleState = ModuleStates.Module_State_Shutdown;
                                    // REC: Close the socket to ensure that the
                                    // disconnect event is triggered:
                                    _socket.Close();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitiateRecv(IoContext rxContext)
        {
            rxContext._ipcSocket.BeginReceive(rxContext._ipcBuffer.Buffer, rxContext._ipcBuffer.WrIndex,
                rxContext._ipcBuffer.Remaining(), 0, CompleteRecv, rxContext);
        }

        /// <summary>
        /// The CompleteRecv method is invoked in response to
        /// an asynchronous receive operation completing.
        /// </summary>
        /// <param name="ar"></param>
        private void CompleteRecv(IAsyncResult ar)
        {
            // REC: Retrieve the IO context that is associated with the
            // original asynchronous receive operation.
            IoContext rxContext = ar.AsyncState as IoContext;
            if (rxContext != null)
            {
                try
                {
                    // REC: Get the total number of bytes that were read
                    // from the peer system by this operation:
                    int rxBytes = rxContext._ipcSocket.EndReceive(ar);
                    if (rxBytes > 0)
                    {
                        // REC: Adjust the write index in the message block:
                        rxContext._ipcBuffer.WrIndex = rxContext._ipcBuffer.WrIndex + rxBytes;
                        
                        // REC: Offload the received data onto a separate thread:
                        if (this._rxBuffering == true)
                        {
                            Socket rxSocket = rxContext._ipcSocket;
                            lock (this._synchRxPendingQueue)
                            {
                                // REC: Push the received data onto the queue so that
                                // it can be picked up by the secondary receive thread
                                // for further processing:
                                this._rxPendingQueue.Enqueue(rxContext);

                                // REC: If the queue was empty prior to this context
                                // being added to it, we need to signal the secondary
                                // receive thread so that it can start processing the
                                // data again:
                                if (this._rxPendingQueue.Count == 1)
                                {
                                    this._eventRxBuffer.Set();
                                }
                            }

                            // REC: Attempt to pull another context from the context
                            // queue so that we can receive more data:
                            IoContext rxNext;
                            if (this._rxContextQueue.Count > 0)
                            {
                                rxNext = this._rxContextQueue.Dequeue();
                            }
                            else
                            {
                                // REC: Note that this is BAD and will result in the 
                                // exhaustion of all the system's memory if there is a
                                // continuous problem with the speed at which messages
                                // are being consumed on the receive thread...
                                rxNext = new IoContext(rxSocket, new VfxMsgBlock(8192));
                            }

                            // REC: Initiate a new receive operation, using the next
                            // available receive context:
                            InitiateRecv(rxNext);
                        }
                        else
                        {
                            // REC: If receive buffering has not been enabled then the
                            // data is dispatched directly to the module's owner:
                            EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                            if (tmp != null)
                            {
                                tmp(this, new VfxIpcEventArgs(rxContext._ipcBuffer));

                                // REC: Adjust the buffer to take into consideration
                                // any data that was read by the module's owner:
                                rxContext._ipcBuffer.Crunch();

                                // REC: Initiate another receive operation using the
                                // same receive buffer that was just processed:
                                InitiateRecv(rxContext);
                            }
                        }
                    }
                    else
                    {
                        _moduleState = ModuleStates.Module_State_Shutdown;

                        // REC: This might happen as the result of the 
                        // user closing the session down.
                        EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                        if (tmp != null)
                        {
                            VfxIpcEventArgs dxArgs = new VfxIpcEventArgs(VfxIpcEventTypes.Event_Session_Closed);
                            tmp(this, dxArgs);
                        }
                    }
                }
                catch (SocketException)
                {
                    _moduleState = ModuleStates.Module_State_Shutdown;

                    // REC: This might happen as the result of the 
                    // user closing the session down.
                    EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                    if (tmp != null)
                    {
                        VfxIpcEventArgs dxArgs = new VfxIpcEventArgs(VfxIpcEventTypes.Event_Session_Closed);
                        tmp(this, dxArgs);
                    }
                }
                catch (ObjectDisposedException)
                {
                    _moduleState = ModuleStates.Module_State_Shutdown;

                    // REC: This happens if the socket gets closed
                    // locally rather than being closed by the peer.
                    EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                    if (tmp != null)
                    {
                        VfxIpcEventArgs dxArgs = new VfxIpcEventArgs(VfxIpcEventTypes.Event_Session_Closed);
                        tmp(this, dxArgs);
                    }
                }
            }
        }

        /// <summary>
        /// The HandleReceive_Entrypoint method is the entrypoint for
        /// the buffering thread that is started by the module if the
        /// module's owner has enabled incoming IO buffering.
        /// </summary>
        /// <param name="context"></param>
        private void HandleReceive_Entrypoint(object context)
        {
            // REC: The fragment buffer is used when there is residual
            // data remaining from a previously processed buffer. This
            // is allocated at 2x the receive block size, in order for
            // reducing the need to "expand" the fragment buffer when
            // data is left over from a receive operation:
            VfxMsgBlock rxFragment = new VfxMsgBlock(16384);

            while (true)
            {
                // REC: Lock the pending receive queue and check whether or not
                // there are any receive blocks waiting to be processed:
                IoContext rxPending = null;
                lock (this._synchRxPendingQueue)
                {
                    if (this._rxPendingQueue.Count > 0)
                    {
                        rxPending = this._rxPendingQueue.Dequeue();
                    }
                }

                if (rxPending != null)
                {
                    // REC: If there is data in the fragment buffer
                    // then we need to append the data from the incoming
                    // receive context to it:
                    if (rxFragment.Length() > 0)
                    {
                        rxFragment.Append(rxPending._ipcBuffer);

                        // REC: Dispatch from the fragment buffer instead
                        // of from the receive context:
                        EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                        if (tmp != null)
                        {
                            tmp(this, new VfxIpcEventArgs(rxFragment));
                            rxFragment.Crunch();
                        }

                        // REC: Reset the pointers in the receive context
                        // since its data has been copied to the fragment
                        // buffer for subsequent processing:
                        rxPending._ipcBuffer.RdIndex = rxPending._ipcBuffer.WrIndex = 0;

                    }
                    else
                    {
                        // REC: There is no fragment remaining from the previous
                        // receive operation, so we can just dispatch directly from
                        // the received context:
                        EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
                        if (tmp != null)
                        {
                            tmp(this, new VfxIpcEventArgs(rxPending._ipcBuffer));
                            rxPending._ipcBuffer.Crunch();
                        }

                        // REC: Determine if there is a fragment in the buffer
                        // so that we can chain it to subsequent blocks:
                        if (rxPending._ipcBuffer.Length() > 0)
                        {
                            // REC: There is a fragment of a message remaining
                            // in the current buffer, so it has to be copied into
                            // the fragment buffer for further processing:
                            rxFragment.Append(rxPending._ipcBuffer);

                            // REC: Reset the points in the pending receive context
                            // since it has been copied into the fragment buffer:
                            rxPending._ipcBuffer.RdIndex = 0;
                            rxPending._ipcBuffer.WrIndex = 0;
                        }
                    }

                    // REC: Put the receive context back into the queue so
                    // that it can be used by subsequent receive operations:
                    lock (this._rxContextQueue)
                    {
                        this._rxContextQueue.Enqueue(rxPending);
                    }

                }
                else
                {
                    // REC: A message block wasn't available for us on this
                    // iteration, so just wait until one is added to the queue:
                    this._eventRxBuffer.WaitOne();
                }
            }
        }
    }
}
