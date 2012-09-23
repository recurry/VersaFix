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
    sealed class VfxTcpServerSession : IVfxIpcSession
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
        /// The identifier that is associated with the session.
        /// </summary>
        private System.Guid _instanceGuid;

        /// <summary>
        /// The InstanceId property provides access to the unique
        /// identifier that is assigned to an instance of a session.
        /// </summary>
        public System.Guid InstanceId
        {
            get { return _instanceGuid; }
        }

        public VfxTcpServerSession(System.Guid instanceGuid)
        {
            _instanceGuid = instanceGuid;
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
            _ipcModule.HandleTx(mb);
        }

        /// <summary>
        /// The Shutdown method is invoked to request that the
        /// session terminate the connection to the peer system.
        /// </summary>
        public void Shutdown()
        {
            _ipcModule.Shutdown(false);
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
            args.Token = _instanceGuid.ToString();
            EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
            if (tmp != null)
            {
                tmp(this, args);
            }
        }

        #endregion
    }
}
