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

namespace VfxEngine.FixApp
{
    /// <summary>
    /// The IVfxFixApp interface defines the interface for an
    /// instance of a VersaFix FIX application. An instance of
    /// an implementation of this interface can be provided to
    /// an instance of VfxFixEngine as a service interface for
    /// handling events that occur on a FIX session.
    /// </summary>
    public interface IVfxFixApp
    {
        /// <summary>
        /// The OnSessionOpened callback method is invoked
        /// to notify the application that a connection has
        /// been established between it and a peer system.
        /// 
        /// NOTE: The OnSessionOpened event may be redundant
        /// in some cases, depending on the implementation of
        /// the IPC layer the system is using. It should never
        /// be used as an indication that the FIX session itself
        /// is ready to process messages - it is only to notify
        /// the application that a connection, of some sort, has
        /// been established with a peer system. An application
        /// should never attempt to send FIX messages to its peer
        /// until it has received the OnSessionLogon callback.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the callback relates to.
        /// </param>
        void OnSessionOpened(IVfxFixAppSession session);

        /// <summary>
        /// The OnSessionLogon callback method is invoked
        /// when an instance of a FIX session successfully
        /// completes a LOGON with the peer system.
        /// </summary>
        /// <param name="session">
        /// The FIX session the callback relates to.
        /// </param>
        /// <param name="msg">
        /// The LOGON message that was received from the
        /// peer system the session is communicating with.
        /// </param>
        void OnSessionLogon(IVfxFixAppSession session, FixMessage msg);


        /// <summary>
        /// The OnSessionLogout callback method is invoked
        /// when an instance of a FIX session successfully
        /// completes a LOGOUT with the peer system.
        /// </summary>
        /// <param name="session">
        /// The FIX session the callback relates to.
        /// </param>
        /// <param name="msg">
        /// The LOGOUT message that was received from the
        /// peer system the session is communicating with.
        /// </param>
        void OnSessionLogout(IVfxFixAppSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionRxAdmMessage callback method is invoked
        /// when a session receives a session layer message from
        /// the peer system it's communicating with.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        void OnSessionRxAdmMessage(IVfxFixAppSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionRxAppMessage callback method is invoked
        /// when the session receives an application layer message
        /// from the peer system it's communicating with.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        void OnSessionRxAppMessage(IVfxFixAppSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionTxAdmMessage callback method is invoked
        /// when a session is getting ready to dispatch a session
        /// layer message to the peer system.
        /// </summary>
        /// <param name="session">
        /// The session that is about to dispatch a session layer
        /// message to its peer.
        /// </param>
        /// <param name="msg">
        /// The session layer message the session is going to send.
        /// </param>
        void OnSessionTxAdmMessage(IVfxFixAppSession session, FixMessage msg);

        /// <summary>
        /// The OnAppMessage callback method is invoked when
        /// a session is getting ready to dispatch an application layer
        /// message to the peer session.
        /// </summary>
        /// <param name="session">
        /// The session that is about to dispatch an application layer
        /// message to its peer.
        /// </param>
        /// <param name="msg">
        /// The application layer message that is going to be sent.
        /// </param>
        void OnSessionTxAppMessage(IVfxFixAppSession session, FixMessage msg);

        /// <summary>
        /// The OnSessionclosed callback is invoked when a session
        /// has been shutdown as a result of either the peer system
        /// disconnecting, or the application shutting it down.
        /// 
        /// NOTE: The OnSessionClosed callback may be redundant in
        /// some cases, depending on the implementation of the IPC
        /// layer that is being used by the system. This callback is
        /// similar to the OnSessionOpened callback in that it is not
        /// related to the status of the FIX session, but rather the
        /// status of the actual IPC connection to the peer system.
        /// </summary>
        /// <param name="session">
        /// The session that the event relates to.
        /// </param>
        void OnSessionClosed(IVfxFixAppSession session);
    }

}