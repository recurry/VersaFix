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

using System.Xml;
using System.Xml.XPath;

using VfxEngine.Ipc;
using VfxEngine.Settings;

namespace VfxEngine.Ipc.Endpoints
{
    /// <summary>
    /// The VfxTcpClientEndpoint class provides an implementation
    /// of the IVfxEndpoint interface that creates a connection to
    /// a server side system for communicating over a TCP socket.
    /// </summary>
    public sealed class VfxTcpClientEndpoint : IVfxEndpoint
    {
        /// <summary>
        /// The IPEndPoint instance that defines the address of
        /// the peer system the IPC endpoint will connect to.
        /// </summary>
        private IPEndPoint _tcpEndpoint;

        /// <summary>
        /// The TCP connector that the IPC endpoint will use to
        /// establish the connection to the peer system.
        /// </summary>
        private VfxTcpConnector _tcpConnector = new VfxTcpConnector();

        /// <summary>
        /// The IPC session handler that handles events which are
        /// dispatched by the TCP module the connector creates when
        /// it establishes the outbound connection; the session just
        /// routes its IPC events back to the endpoint which then 
        /// dispatches them to its owner. 
        /// </summary>
        private VfxTcpClientSession _ipcSession = new VfxTcpClientSession();

        /// <summary>
        /// Flag that indicates whether or not an instance of the
        /// endpoint has been initialized.
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// The flag that indicates whether or not a connection has
        /// been established to the peer system.
        /// </summary>
        private bool _established = false;

        /// <summary>
        /// The flag that indicates whether or not the endpoint should
        /// keep trying to establish a connection.
        /// </summary>
        private bool _reconnect = false;

        #region IVfxEndpoint Members

        /// <summary>
        /// The EventDispatch event is implemented to dispatch
        /// events from an endpoint to its subscribers. See the
        /// definition of VfxIpcEventArgs for additional details
        /// relating to the types and contents of events that are
        /// expected to be dispatched by an endpoint.
        /// </summary>
        public event EventHandler<VfxIpcEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method is invoked to start the endpoint
        /// and initiate a connection to the configured peer.
        /// </summary>
        public void Activate()
        {
            if (_initialized == true)
            {
                _reconnect = true;

                // REC: Bind to the dispatch event on the IPC session
                // in order to receive IPC events and forward them to
                // the endpoint's owner:
                _ipcSession.EventDispatch += HandleIpc_Dispatch;

                // REC: Initiate the connection attempt:
                _tcpConnector.Connect(_tcpEndpoint, _ipcSession);

            }
            else
            {
                throw new InvalidOperationException("The endpoint has not been initialized.");
            }
        }

        /// <summary>
        /// The Init method is invoked to initialize an instance
        /// of a VfxTcpClientEndpoint with configuration settings
        /// from a configuration file/metabase.
        /// </summary>
        /// <param name="settings">
        /// The section of the configuration metabase that contains
        /// the configuration settings for the endpoint instance.
        /// </param>
        public void Initialize(XmlDocument settings)
        {
            XPathNavigator xpn = settings.CreateNavigator();
            XPathNodeIterator xpi = xpn.Select("/Endpoint/Settings/Setting[@name='Tcp.Connector.Addr']");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                string clientAddr = xpi.Current.GetAttribute("content", "");
                if (!string.IsNullOrEmpty(clientAddr))
                {
                    xpi = xpn.Select("/Endpoint/Settings/Setting[@name='Tcp.Connector.Port']");
                    if ((xpi.Count == 1) && (xpi.MoveNext()))
                    {
                        string clientPort = xpi.Current.GetAttribute("content","");
                        if (!string.IsNullOrEmpty(clientPort))
                        {
                            IPAddress ipAddress = IPAddress.Any;
                            if (clientAddr.CompareTo("*") != 0)
                            {
                                ipAddress = IPAddress.Parse(clientAddr);
                            }

                            this._tcpEndpoint = new IPEndPoint(ipAddress, int.Parse(clientPort));
                            this._initialized = true;
                        }
                    }

                    xpi = xpn.Select("/Endpoint/Settings/Setting[@name='Tcp.Receive.Buffering']");
                    if ((xpi.Count == 1) && (xpi.MoveNext()))
                    {
                        string rxBuffering = xpi.Current.GetAttribute("content", "");
                        if (!string.IsNullOrEmpty(rxBuffering))
                        {
                            if ((rxBuffering.ToLower().CompareTo("t") == 0) || (rxBuffering.ToLower().CompareTo("true") == 0))
                            {
                                // REC: Configure the acceptor to enable receive
                                // buffering on each session that is initiated in
                                // response to an incoming connection:
                                this._tcpConnector.RxBuffering = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The Send method is invoked to send data to the
        /// peer system the endpoint is connected to.
        /// </summary>
        /// <param name="sessionId">
        /// Ignored - server endpoints require a specific session
        /// identifier be supplied when data is sent, but this is
        /// not required for client implementations as they manage
        /// only a single connection.
        /// </param>
        /// <param name="mb">
        /// The data that is to be sent to the peer system.
        /// </param>
        public void Send(string sessionId, VfxMsgBlock mb)
        {
            if (_ipcSession != null)
            {
                _ipcSession.Send(mb);
            }
        }

        /// <summary>
        /// The Shutdown method is invoked to terminate the connection
        /// that the endpoint has established to its configured peer.
        /// </summary>
        public void Shutdown()
        {
            if (_ipcSession != null)
            {
                _ipcSession.Shutdown();
            }
        }

        public void Shutdown(string sessionId)
        {
            return;
        }

        #endregion

        /// <summary>
        /// The HandleIpc_Dispatch method is invoked in response
        /// to the IPC session handler dispatching an IPC event to
        /// the endpoint. The event is simply forwarded to the owner
        /// of the endpoint instance for processing.
        /// </summary>
        /// <param name="sender">
        /// The IPC session that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The details of the IPC event being dispatched.
        /// </param>
        private void HandleIpc_Dispatch(object sender, VfxIpcEventArgs args)
        {
            // REC: The incoming event from the underlying IPC session
            // is simply forwarded to the endpoint's owner:
            EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
            if (tmp != null)
            {
                tmp(this, args);
            }
        }

        /// <summary>
        /// The HandleTcpConnector_Dispatch event handler is invoked
        /// in response to the events from the connector the endpoint
        /// is using to establish a connection.
        /// </summary>
        /// <param name="sender">
        /// The VfxTcpConnector instance that is dispatching the
        /// event to the endpoint.
        /// </param>
        /// <param name="args">
        /// The details of the event being dispatched.
        /// </param>
        private void HandleTcpConnector_Dispatch(object sender, VfxTcpConnectorEventArgs args)
        {
            switch (args.EventType)
            {
                case VfxTcpConnectorEventTypes.Event_Connect_Success:
                    HandleTcpConnector_Success(sender, args);
                    break;
                case VfxTcpConnectorEventTypes.Event_Connect_Failure:
                    HandleTcpConnector_Failure(sender, args);
                    break;
            }
        }

        /// <summary>
        /// The HandleTcpConnector_Success event handler is invoked
        /// in response to the tcp connector establishing a connection
        /// to the peer system.
        /// </summary>
        /// <param name="sender">
        /// The connector that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The event details that are associated with the event.
        /// </param>
        private void HandleTcpConnector_Success(object sender, VfxTcpConnectorEventArgs args)
        {
            // REC: Translate the connector's success notification
            // into the corresponding IPC event and dispatch it:
            EventHandler<VfxIpcEventArgs> tmpDispatch = EventDispatch;
            if (tmpDispatch != null)
            {
                VfxIpcEventArgs tmpArgs = new VfxIpcEventArgs(VfxIpcEventTypes.Event_Connect_Success);
                tmpDispatch(this, tmpArgs);
            }
        }

        /// <summary>
        /// The HandleTcpConnector_Failure event handler is invoked 
        /// in response to the connector not being able to establish
        /// a connection to the peer system.
        /// </summary>
        /// <param name="sender">
        /// The connector that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The event details that are associated with the event.
        /// </param>
        private void HandleTcpConnector_Failure(object sender, VfxTcpConnectorEventArgs args)
        {
            // REC: Translate the connector's failure notification
            // into the corresponding IPC event and dispatch it:
            EventHandler<VfxIpcEventArgs> tmpDispatch = EventDispatch;
            if(tmpDispatch != null)
            {
                VfxIpcEventArgs tmpArgs = new VfxIpcEventArgs(VfxIpcEventTypes.Event_Connect_Failure);
                tmpDispatch(this, tmpArgs);
            }
        }
    }
}
