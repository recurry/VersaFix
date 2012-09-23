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

using System.Xml;
using System.Xml.XPath;

using VfxEngine.Ipc;
using VfxEngine.Settings;

namespace VfxEngine.Ipc.Endpoints
{
    /// <summary>
    /// The VfxTcpServerEndpoint class provides an implementation
    /// of an IPC endpoint that acts as server. The endpoint will
    /// accept incoming connections and then mux/demux IPC events
    /// between each session and the application.
    /// </summary>
    public sealed class VfxTcpServerEndpoint : IVfxIpcSessionFactory, IVfxEndpoint
    {
        /// <summary>
        /// The EventDispatch event is fired by the server endpoint
        /// when IPC events are received from sessions that are being
        /// managed by the endpoint. The VfxIpcEventArgs instance for
        /// the event has its Token parameter set to the identifer of
        /// the session that generated the event so that subscribers
        /// can use the session id to send response messages, etc.
        /// </summary>
        public event EventHandler<VfxIpcEventArgs> EventDispatch;

        /// <summary>
        /// The .NET endpoint that the server endpoint will use
        /// for accepting TCP connections from peers.
        /// </summary>
        private IPEndPoint _tcpEndpoint;

        /// <summary>
        /// The VersaFix TCP acceptor that the server endpoint
        /// will use for accepting connections from peers.
        /// </summary>
        private VfxTcpAcceptor _tcpAcceptor = new VfxTcpAcceptor();

        /// <summary>
        /// The map of all IPC sessions that are being managed by
        /// the endpoint, keyed by their respective identifiers.
        /// </summary>
        private Dictionary<string, VfxTcpServerSession> _mapSessions = new Dictionary<string, VfxTcpServerSession>();

        #region IVfxEndpoint Members

        /// <summary>
        /// The Activate method opens the endpoint. After the
        /// endpoint has been opened, messages can be sent and
        /// received over it until it is shutdown.
        /// </summary>
        public void Activate()
        {
            if (_tcpEndpoint != null)
            {
                _tcpAcceptor.Activate(_tcpEndpoint, this);
            }
            else
            {
                throw new InvalidOperationException("VfxTcpServerEndpoint: The TCP endpoint is not configured!");
            }
        }

        public void Initialize(XmlDocument settings)
        {
            XPathNavigator xpn = settings.CreateNavigator();
            XPathNodeIterator xpi = xpn.Select("/Endpoint/Settings/Setting[@name='Tcp.Acceptor.Addr']");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                string serverAddr = xpi.Current.GetAttribute("content", "");
                if (!string.IsNullOrEmpty(serverAddr))
                {
                    xpi = xpn.Select("/Endpoint/Settings/Setting[@name='Tcp.Acceptor.Port']");
                    if ((xpi.Count == 1) && (xpi.MoveNext()))
                    {
                        string serverPort = xpi.Current.GetAttribute("content", "");
                        if (!string.IsNullOrEmpty(serverPort))
                        {
                            IPAddress ipAddress = IPAddress.Any;
                            if (serverAddr.CompareTo("*") != 0)
                            {
                                ipAddress = IPAddress.Parse(serverAddr);
                            }

                            this._tcpEndpoint = new IPEndPoint(ipAddress, int.Parse(serverPort));
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
                                    this._tcpAcceptor.RxBuffering = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The Send method attempts to send an instance of a
        /// message through the specified endpoint session.
        /// </summary>
        /// <param name="sessionId">
        /// The unique identifier associated with the session
        /// that the message is to be sent to.
        /// </param>
        /// <param name="mb">
        /// The FIX message that is to be sent.
        /// </param>
        public void Send(string sessionId, VfxMsgBlock mb)
        {
            if (_mapSessions.ContainsKey(sessionId))
            {
                _mapSessions[sessionId].Send(mb);
            }
        }

        /// <summary>
        /// The Shutdown method closes the endpoint. After the
        /// endpoint has been shutdown, no further communications
        /// can take place over it.
        /// </summary>
        public void Shutdown()
        {
            // REC: Shutdown the acceptor:
            _tcpAcceptor.Shutdown(_tcpEndpoint);
        }

        /// <summary>
        /// The Shutdown method is invoked to request that the
        /// server endpoint shutdown a specific session.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the session to be shutdown.
        /// </param>
        public void Shutdown(string sessionId)
        {
            if (_mapSessions.ContainsKey(sessionId))
            {
                _mapSessions[sessionId].Shutdown();
            }
        }

        #endregion

        /// <summary>
        /// The HandleDispatch event handler is invoked in response to
        /// one of the IPC sessions dispatching an event to the endpoint.
        /// </summary>
        /// <param name="sender">
        /// The IPC session that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The details of the event being dispatched. The Token property
        /// of the event args instance will be set to the session id of the
        /// session that generated the event so that subscribers know which
        /// of the endpoint's sessions the event originated from.
        /// </param>
        private void HandleDispatch(object sender, VfxIpcEventArgs args)
        {
            EventHandler<VfxIpcEventArgs> tmp = EventDispatch;
            if (tmp != null)
            {
                tmp(this, args);
            }
        }

        #region IVfxIpcSessionFactory Members

        /// <summary>
        /// The CreateSession method creates a new instance of
        /// an IPC handler session that the server endpoint will
        /// manage for multiplexing IPC events to and from a peer
        /// that has connected to the endpoint.
        /// </summary>
        /// <returns></returns>
        public IVfxIpcSession CreateSession()
        {
            // REC: Create a new instance of an IPC session:
            VfxTcpServerSession result = new VfxTcpServerSession(System.Guid.NewGuid());
            // REC: Add the new session to the local map:
            _mapSessions.Add(result.InstanceId.ToString(), result);
            // REC: Bind to the session's event dispatcher:
            result.EventDispatch += HandleDispatch;
            // REC: Return the IPC session handler to the acceptor
            // which will then initialize and activate it:
            return result;
        }

        #endregion


    }
}
