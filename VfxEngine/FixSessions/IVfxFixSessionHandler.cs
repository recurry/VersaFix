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

using VfxEngine.Fix;
namespace VfxEngine.FixSessions
{
    /// <summary>
    /// The IVfxFixSessionHandler interface defines an interface
    /// for an implementation of a callback handler that can be 
    /// provided to an instance of IVfxFixSession as the callback
    /// interface for handling events that occur on the session.
    /// </summary>
    public interface IVfxFixSessionHandler
    {
        /// <summary>
        /// The OnSessionStarted callback method is invoked by an
        /// instance of a session when it is activated.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the event relates to.
        /// </param>
        void OnSessionOpened(IVfxFixSession session);

        /// <summary>
        /// The OnSessionLogon callback method is invoked by an
        /// instance of a session when it completes a logon with
        /// the peer system it's communicating with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing
        /// the callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The FIX logon message (request or response) that was
        /// received from the peer session.
        /// </param>
        void OnSessionLogon(IVfxFixSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionTimeout callback method is invoked by an
        /// instance of a session when it has not received any data
        /// from the peer during its configured timeout interval.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        void OnSessionTimeout(IVfxFixSession session);

        /// <summary>
        /// The OnSessionLogout callback method is invoked by an
        /// instance of a session when it completes a logout with
        /// the peer system it's communicating with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing
        /// the callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The FIX logout message (request or response) that was
        /// received from the peer session.
        /// </param>
        void OnSessionLogout(IVfxFixSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionRxAdmMessage callback is invoked by a session
        /// when it receives an administrative (session) layer message
        /// from the peer session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing
        /// the callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The session layer message that was received.
        /// </param>
        void OnSessionRxAdmMessage(IVfxFixSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionRxAppMessage callback is invoked by a session
        /// when it receives an application layer message from the peer
        /// session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The application message that the session has received
        /// from its corresponding peer session.
        /// </param>
        void OnSessionRxAppMessage(IVfxFixSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionTxAdmMessage callback is invoked by a session
        /// when it is ready to dispatch an administrative message to the
        /// peer session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The administrative message that the session wants to have
        /// sent to its corresponding peer session.
        /// </param>
        void OnSessionTxAdmMessage(IVfxFixSession session, FixMessage msg);


        /// <summary>
        /// The OnSessionTxAppMessage callback is invoked by a session
        /// when it is ready to dispatch an application message to the
        /// peer session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The application message that the session wants to have sent
        /// to its corresponding peer session.
        /// </param>
        void OnSessionTxAppMessage(IVfxFixSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionStopped callback method is invoked in response
        /// to an instance of a session being shutdown.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the event relates to.
        /// </param>
        void OnSessionClosed(IVfxFixSession session);
    }
}
