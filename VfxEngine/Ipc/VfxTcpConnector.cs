using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxTcpConnector class encapsulates the functionality required
    /// to connect an instance of an IPC session handler to a remote peer
    /// over a TCP socket.
    /// </summary>
    public class VfxTcpConnector
    {
        /// <summary>
        /// The EventDispatch event is used by an instance of the
        /// connector to dispatch different events to its owner:
        /// </summary>
        public event EventHandler<VfxTcpConnectorEventArgs> EventDispatch;

        /// <summary>
        /// The IpcState class is an internal class that is used to
        /// maintain context for asynchronous socket operations.
        /// </summary>
        private class IpcState
        {
            /// <summary>
            /// The TCP socket associated with an asynchronous
            /// operation that is being executed.
            /// </summary>
            public Socket IpcSocket;

            /// <summary>
            /// The .NET IPEndPoint associated with an asynchronous
            /// operation that is being executed.
            /// </summary>
            public IPEndPoint IpcEndpoint;

            /// <summary>
            /// The VFX IPC session handler that is associated with
            /// an asynchronous operation that is being executed.
            /// </summary>
            public IVfxIpcSession IpcSession;

            /// <summary>
            /// Initializes a new instance of IpcState with the
            /// specified parameters.
            /// </summary>
            /// <param name="socket">
            /// The TCP socket that is associated with the 
            /// asynchronous operation.
            /// </param>
            /// <param name="endpoint">
            /// The .NET endpoint that is associated with the
            /// asynchronous operation.
            /// </param>
            /// <param name="session">
            /// The VFX IPC handler that is associated with the
            /// asynchronous operation.
            /// </param>
            public IpcState(Socket socket, IPEndPoint endpoint, IVfxIpcSession session)
            {
                IpcSocket = socket;
                IpcEndpoint = endpoint;
                IpcSession = session;
            }
        }

        public bool RxBuffering { get; set; }

        /// <summary>
        /// Initializes a new instance of VfxTcpConnector with 
        /// the appropriate default settings.
        /// </summary>
        public VfxTcpConnector()
        {
            this.RxBuffering = false;
        }

        /// <summary>
        /// The Connect method attempts to connect the specified
        /// session handler to the peer system that is accepting
        /// connections on the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint of a peer session that the connector is
        /// to connect the supplied session handler to.
        /// </param>
        /// <param name="session">
        /// The VFX IPC session handler that is to be connected 
        /// to the peer system at the specified endpoint.
        /// </param>
        public void Connect(IPEndPoint endpoint, IVfxIpcSession session)
        {
            // REC: Create an instance of a socket that will be
            // used to connect to the peer system:
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            // REC: Create an instance of the IpcState class that
            // will provide the context for the asynch request:
            IpcState ipcState = new IpcState(socket, endpoint, session);

            // REC: Initiate the asynchronous connection request:
            ipcState.IpcSocket.BeginConnect(ipcState.IpcEndpoint, HandleConnect, ipcState);
        }

        /// <summary>
        /// The HandleConnect method is invoked by an asynchronous
        /// connection attempt to inform the connector that it has
        /// been completed. The connector will take the appropriate
        /// action depending on the resulting state of the request.
        /// </summary>
        /// <param name="ar">
        /// The .NET asynchronous result details.
        /// </param>
        private void HandleConnect(IAsyncResult ar)
        {
            // REC: Retrieve the original IPC state information
            // that was provided to the asynchronous operation:
            IpcState ipcState = ar.AsyncState as IpcState;
            if (ipcState != null)
            {
                try
                {
                    ipcState.IpcSocket.EndConnect(ar);
                    // REC: Dispatch the connect success event to the
                    // subscribers so that they can determine whether 
                    // or not the connection should go forward.
                    EventHandler<VfxTcpConnectorEventArgs> tmpDispatch = EventDispatch;
                    if (tmpDispatch != null)
                    {
                        VfxTcpConnectorEventArgs connectArgs = new VfxTcpConnectorEventArgs();
                        connectArgs.EventType = VfxTcpConnectorEventTypes.Event_Connect_Success;
                        connectArgs.EventSocket = ipcState.IpcSocket;
                        connectArgs.EventEndpoint = ipcState.IpcEndpoint;
                        tmpDispatch(this, connectArgs);

                        // REC: Check the cancellation flag to determine if
                        // any subscriber wants the connection canceled:
                        if (connectArgs.EventCancel == true)
                        {
                            ipcState.IpcSocket.Close();
                            return;
                        }
                    }


                    // REC: Create a new instance of VfxTcpModule that
                    // will be used for processing IO over the socket:
                    VfxTcpModule tcpModule = new VfxTcpModule();
                    tcpModule.Init(ipcState.IpcSocket, this.RxBuffering);

                    // REC: Pass the initialized module to the instance
                    // of the VFX IPC session that is associated with the
                    // connection request:
                    ipcState.IpcSession.Init(tcpModule);
                }
                catch (System.Exception x)
                {
                    EventHandler<VfxTcpConnectorEventArgs> tmpDispatch = EventDispatch;
                    if (tmpDispatch != null)
                    {
                        VfxTcpConnectorEventArgs tmpArgs = new VfxTcpConnectorEventArgs();
                        tmpArgs.EventType = VfxTcpConnectorEventTypes.Event_Connect_Failure;
                        tmpArgs.EventEndpoint = ipcState.IpcEndpoint;
                        tmpArgs.EventException = x;
                        tmpDispatch(this, tmpArgs);
                    }
                }
            }
        }
    }
}
