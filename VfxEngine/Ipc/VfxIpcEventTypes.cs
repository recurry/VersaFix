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

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxIpcEventTypes enumeration provides a list of all
    /// event types that can be dispatched by an IPC module.
    /// </summary>
    public enum VfxIpcEventTypes
    {
        /// <summary>
        /// The Event_Connect_Success event type indicates that a
        /// connection has been established to a peer system. 
        /// </summary>
        Event_Connect_Success,
        /// <summary>
        /// The Event_Connect_Failure event type indicates that a
        /// connection could not be established to a peer system.
        /// </summary>
        Event_Connect_Failure,
        /// <summary>
        /// The Event_Opened event type indicates that an instance
        /// of an IPC module has been activated and messages can be
        /// sent and received from the peer system.
        /// </summary>
        Event_Session_Opened,
        /// <summary>
        /// The Event_Closed event type indicates that an instance
        /// of an IPC module has been shutdown and messages can no
        /// longer be sent or received from the peer system.
        /// </summary>
        Event_Session_Closed,
        /// <summary>
        /// The Event_Session_Logging event type indicates that an instance
        /// of an IPC module is dispatching a log message. The text
        /// of the message can be accessed via the EventText property
        /// in the VfxIpcEventArgs instance being dispatched.
        /// </summary>
        Event_Session_Logging,
        /// <summary>
        /// The Event_Session_Message event type indicates that an instance
        /// of an IPC module is dispatching data that has been received
        /// from the peer system it is associated with. The actual data
        /// of the message can be accessed via the EventData property
        /// in the VfxIpcEventArgs instance being dispatched.
        /// </summary>
        Event_Session_Message,
        /// <summary>
        /// The Event_Session_Exception event type indicates that an instance
        /// of an IPC module is dispatching an exception that was thrown
        /// during a communication operation. The actual exception that
        /// was thrown can be accessed via the EventException property
        /// in the VfxIpcEventArgs instance being dispatched.
        /// </summary>
        Event_Session_Exception
    }
}
