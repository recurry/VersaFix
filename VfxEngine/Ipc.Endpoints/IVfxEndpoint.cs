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

using System.Xml;
using System.Xml.XPath;

using VfxEngine.Settings;

namespace VfxEngine.Ipc.Endpoints
{
    /// <summary>
    /// The IVfxEndpoint interface defines a common interface
    /// that can be implemented by specific types of endpoints
    /// for sending and receiving data between peers.
    /// </summary>
    public interface IVfxEndpoint
    {
        /// <summary>
        /// The EventDispatch event is implemented to dispatch
        /// events from an endpoint to its subscribers. See the
        /// definition of VfxIpcEventArgs for additional details
        /// relating to the types and contents of events that are
        /// expected to be dispatched by an endpoint.
        /// </summary>
        event EventHandler<VfxIpcEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method opens the endpoint. After the
        /// endpoint has been opened, messages can be sent and
        /// received over it until it is shutdown.
        /// </summary>
        void Activate();

        /// <summary>
        /// The Initialize method initializes the endpoint with
        /// implementation specific configuration settings.
        /// </summary>
        /// <param name="settings">
        /// The configuration settings that determine how the
        /// endpoint will be initialized.
        /// </param>
        void Initialize(XmlDocument settings);

        /// <summary>
        /// The Send method attempts to send a message to the
        /// system the endpoint is connected to. If an endpoint
        /// is managing multiple sessions the data can be routed
        /// to a specific session by specifying the session's id
        /// in the sessionId parameter; this is done with server
        /// endpoints that are accepting multiple connections.
        /// </summary>
        /// <param name="sessionId">
        /// The identifier of the specific IPC session that the
        /// message is to be directed to, or null if the endpoint
        /// is not managing multiple sessions.
        /// </param>
        /// <param name="mb">
        /// The message data that is to be sent.
        /// </param>
        void Send(string sessionId, VfxMsgBlock mb);

        /// <summary>
        /// The Shutdown method closes the endpoint. After the
        /// endpoint has been shutdown, no further communications
        /// can take place over it.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// The Shutdown method is invoked to request that the
        /// endpoint shutdown a specific session.
        /// </summary>
        /// <param name="sessionId">
        /// The session identifier of the session to shutdown.
        /// </param>
        void Shutdown(string sessionId);


    }
}
