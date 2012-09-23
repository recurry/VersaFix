using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VfxEngine.Ipc;

namespace VfxEngine.Ipc.Endpoints
{
    /// <summary>
    /// The VfxTcpServerSession class provides an implementation
    /// of the IPC event handling interface for a session that is
    /// created by the VfxTcpAcceptor when a client connects to an
    /// instance of a VfxTcpServerEndpoint. The IPC session simply
    /// forwards IPC events from the peer to the server endpoint.
    /// </summary>
    public sealed class VfxTcpClientSession : IVfxIpcSession
    {
        /// <summary>
        /// The EventDispatch event forwards all events that are
        /// received by the session to its subscribers; typically
        /// the VfxTcpServerEndpoint that owns the session.
        /// </summary>
        public event EventHandler<VfxIpcEventArgs> EventDispatch;

        /// <summary>
        /// The IPC module that is associated with the session.
        /// </summary>
        private IVfxIpcModule _ipcModule;

        /// <summary>
        /// Initializes an instance of VfxTcpClientSession with
        /// the appropriate default settings.
        /// </summary>
        public VfxTcpClientSession()
        {

        }

        /// <summary>
        /// The Send method attempts to send the specified data
        /// out to a peer system through the IPC module that is
        /// associated with the session.
        /// </summary>
        /// <param name="mb">
        /// The message block that contains the data that is to
        /// be transmitted through the IPC module.
        /// </param>
        public void Send(VfxMsgBlock mb)
        {
            if (_ipcModule != null)
            {
                _ipcModule.HandleTx(mb);
            }
        }

        /// <summary>
        /// The Shutdown method is invoked to request that the
        /// session close the underlying IPC connection.
        /// </summary>
        public void Shutdown()
        {
            if (_ipcModule != null)
            {
                _ipcModule.Shutdown(false);
            }
        }


        #region IVfxIpcSession Members

        /// <summary>
        /// The Init method initializes the instance of the
        /// session with the corresponding IpcModule that is
        /// provided by the VfxTcpAcceptor implementation.
        /// </summary>
        /// <param name="ipcModule">
        /// The IPC module that represents the communication
        /// session with the peer system.
        /// </param>
        public void Init(IVfxIpcModule ipcModule)
        {
            // REC: Maintain a reference to the associated
            // instance of an IPC module:
            _ipcModule = ipcModule;
            // REC: Bind to the IPC module's event dispatcher
            // for forwarding IPC events to the endpoint that
            // owns the session:
            _ipcModule.EventDispatch += HandleDispatch;
            // REC: Activate the IPC module:
            _ipcModule.Activate();

        }

        /// <summary>
        /// The HandleDispatch event handler is invoked in response
        /// to an IPC event being dispatched to the session by the
        /// session's associated IPC module. The session assigns its
        /// unique identifier to the event and then forwards the event
        /// up to the server endpoint that owns the session.
        /// </summary>
        /// <param name="sender">
        /// The IPC module that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The details of the event being dispatched.
        /// </param>
        private void HandleDispatch(object sender, VfxIpcEventArgs args)
        {
            //args.Token = _instanceGuid.ToString();
            EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
            if (tmp != null)
            {
                tmp(this, args);
            }
        }

        #endregion
    }
}
