﻿//************************************************************************
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

using VfxEngine.Fix;
using VfxEngine.Ipc;
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixSessions
{
    /// <summary>
    /// The IVfxFixSession interface defines the interface for a state
    /// machine that manages a FIX session. The VersaFix system uses an
    /// implementation of this interface for each FIX session that it is
    /// managing. As messages are received from an IPC endpoint, they are
    /// directed to the appropriate instance of an implementation of this
    /// interface, which then processes the incoming data and generates the
    /// appropriate responses at either the session or application layer of
    /// the session, depending on the situation.
    /// </summary>
    public interface IVfxFixSession
    {
        /// <summary>
        /// The InstanceId property must return an instance of a
        /// unique identifier that is associated with the session.
        /// </summary>
        System.Guid InstanceId { get; }

        /// <summary>
        /// The Init method initializes an implementation of the 
        /// interface with the specified parameters. The instance
        /// must be initialized before it can be used.
        /// </summary>
        /// <param name="services">
        /// A reference to a service container that provides the
        /// services which are required by the session.
        /// </param>
        /// <param name="handler">
        /// A reference to an implementation of the callback interface
        /// for a session. The session implementation issues callbacks
        /// to this callback handler in response to various events that
        /// occur on the session. Refer to the IVfxFixSessionHandler's 
        /// documentation for more detail on the callbacks which may be
        /// generated by a session implementation.
        /// </param>
        void Init(IVfxServices services, IVfxFixSessionHandler handler);

        /// <summary>
        /// The HandleConnect method is invoked to inform an instance
        /// of a session that it a connection has been established to
        /// the peer system. The session should begin handling events
        /// from the peer session immediately.
        /// </summary>
        void HandleConnect();

        /// <summary>
        /// The HandleDisconnect method is invoked by the owner of
        /// an instance of a session to inform the session that it
        /// has been disconnected from a peer system.
        /// </summary>
        void HandleDisconnect();

        /// <summary>
        /// The HandleRxMessage method is invoked by the owner of
        /// an instance of a session to inform it that data has been
        /// received from the peer system it is communicating with.
        /// </summary>
        /// <param name="mb">
        /// The message block that contains the data which has been
        /// received from the peer system.
        /// </param>
        void HandleRxMessage(VfxMsgBlock mb);

        /// <summary>
        /// The HandleTxMessage method is invoked by the owner of
        /// an instance of a session to inform it that a message is
        /// ready to be sent to the peer system.
        /// </summary>
        /// <param name="msg">
        /// The FIX message that the session's owner wants to send
        /// to the peer system that the session relates to.
        /// </param>
        void HandleTxMessage(FixMessage msg);

        /// <summary>
        /// The HandleShutdown method is invoked to request that
        /// an instance of a session shut itself down. The session
        /// must carry out the shutdown in accordance with the FIX
        /// version it is implementing, e.g. sending a logout and
        /// waiting for the logout response message, etc.
        /// </summary>
        void HandleShutdown();
    }
}
