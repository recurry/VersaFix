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
    /// The VfxIpcEventArgs class encapsulates all of the
    /// details of an IPC event that has been dispatched by
    /// an instance of an IPC module.
    /// </summary>
    public sealed class VfxIpcEventArgs : EventArgs
    {
        private string _token;

        /// <summary>
        /// The Token property provides access to a unique token
        /// that is associated with the IPC module or event that
        /// is being dispatched. This token is a general purpose
        /// identifier that can have different meanings depending
        /// on the context in which an event is being raised.
        /// </summary>
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        private VfxIpcEventTypes _eventType;

        /// <summary>
        /// The EventType property provides access to the type of
        /// event that is being dispatched. This is used to inform
        /// subscribers to IPC module events of the actual type of
        /// the event that the IPC module is raising.
        /// </summary>
        public VfxIpcEventTypes EventType
        {
            get { return _eventType; }
        }

        private string _eventText;

        /// <summary>
        /// The EventText property provides access to a text string
        /// that is associated with the event. This property is only
        /// relevant if the EventType is set to Event_Logging, and is
        /// used to dispatch a logging or information message to the
        /// owner of an IPC module.
        /// </summary>
        public string EventText
        {
            get { return _eventText; }
        }

        private VfxMsgBlock _eventData;

        /// <summary>
        /// The EventData property provides access to a message block
        /// that contains data that has been received from the system
        /// that an IPC module is communicating with. This property is
        /// only relevant if the EventType is set to Event_Message.
        /// </summary>
        public VfxMsgBlock EventData
        {
            get { return _eventData; }
        }


        private Exception _eventException;

        /// <summary>
        /// The EventException property provides access to an exception
        /// that has been thrown by an IPC module. This property is only
        /// relevant if the EventType is set to Event_Exception.
        /// </summary>
        public Exception EventException
        {
            get { return _eventException; }
        }


        /// <summary>
        /// Initializes an instance of VfxIpcEventArgs with a specific
        /// event type. This constructor should only be used in order to
        /// inform subscribers of an event that does not have any data, 
        /// text, or exception associated with it.
        /// </summary>
        /// <param name="type">
        /// The type of event that is being dispatched.
        /// </param>
        public VfxIpcEventArgs(VfxIpcEventTypes type)
        {
            _eventType = type;
        }

        /// <summary>
        /// Initializes an instance of VfxIpcEventArgs with a specific
        /// token and event type. This constructor should only be used 
        /// when informating subscribers of an event that does not have
        /// any data, text, or exception associated with it.
        /// </summary>
        /// <param name="token">
        /// The unique token associated with the event.
        /// </param>
        /// <param name="type">
        /// The type of event being dispatched.
        /// </param>
        public VfxIpcEventArgs(string token, VfxIpcEventTypes type)
        {
            _token = token;
            _eventType = type;
        }

        /// <summary>
        /// Initializes an instance of VfxIpcEventArgs with a specific
        /// message block that contains data that has been received from
        /// the peer system associated with an IPC module.
        /// </summary>
        /// <param name="msgBlock">
        /// The message block that contains the data.
        /// </param>
        public VfxIpcEventArgs(VfxMsgBlock msgBlock)
        {
            _eventType = VfxIpcEventTypes.Event_Session_Message;
            _eventData = msgBlock;
        }

        /// <summary>
        /// Initializes an instance of VfxIpcEventArgs with a specific
        /// message block that contains data that has been received from
        /// the peer system associated with an IPC module.
        /// </summary>
        /// <param name="token">
        /// The unique token associated with the event.
        /// </param>
        /// <param name="msgBlock">
        /// The message block that contains the data.
        /// </param>
        public VfxIpcEventArgs(string token, VfxMsgBlock msgBlock)
        {
            _eventType = VfxIpcEventTypes.Event_Session_Message;
            _eventData = msgBlock;
            _token = token;
        }

        /// <summary>
        /// Initializes an instance of VfxIpcEventArgs with the
        /// details of an exception that has been thrown.
        /// </summary>
        /// <param name="x">
        /// The details of the exception that was thrown.
        /// </param>
        public VfxIpcEventArgs(Exception x)
        {
            _eventType = VfxIpcEventTypes.Event_Session_Exception;
            _eventException = x;
        }

        /// <summary>
        /// Initializes an instance of VfxIpcEventArgs with the
        /// details of an exception that has been thrown.
        /// </summary>
        /// <param name="token">
        /// The unique token associated with the event.
        /// </param>
        /// <param name="x">
        /// The details of the exception that was thrown.
        /// </param>
        public VfxIpcEventArgs(string token, Exception x)
        {
            _eventType = VfxIpcEventTypes.Event_Session_Exception;
            _eventException = x;
            _token = token;
        }
    }
}
