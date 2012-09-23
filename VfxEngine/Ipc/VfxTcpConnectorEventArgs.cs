using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxTcpConnectorEventArgs class encapsulates the
    /// details of an event that is dispatched by an instance
    /// of the VfxTcpConnector class.
    /// </summary>
    public class VfxTcpConnectorEventArgs : EventArgs
    {
        /// <summary>
        /// The type of the event that is being dispatched.
        /// </summary>
        private VfxTcpConnectorEventTypes _eventType;

        /// <summary>
        /// The EventType property provides access to the
        /// event type that is being dispatched.
        /// </summary>
        public VfxTcpConnectorEventTypes EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        /// <summary>
        /// The .NET socket that is associated with the 
        /// event that is being dispatched.
        /// </summary>
        private Socket _eventSocket;

        /// <summary>
        /// The EventSocket property provides access to the
        /// socket that is associated with the event.
        /// </summary>
        public Socket EventSocket
        {
            get { return _eventSocket; }
            set { _eventSocket = value; }
        }

        /// <summary>
        /// The IP endpoint that is associated with the
        /// event that is being dispatched.
        /// </summary>
        private IPEndPoint _eventEndpoint;

        /// <summary>
        /// The EventEndpoint property provides access to 
        /// the IP endpoint that is associated with the event.
        /// </summary>
        public IPEndPoint EventEndpoint
        {
            get { return _eventEndpoint; }
            set { _eventEndpoint = value; }
        }

        /// <summary>
        /// The relevant exception if one was thrown by the
        /// system during an operation.
        /// </summary>
        private System.Exception _eventException;

        /// <summary>
        /// The EventException property provides access to
        /// the exception that was thrown by the system if
        /// an operation failed.
        /// </summary>
        public System.Exception EventException
        {
            get { return _eventException; }
            set { _eventException = value; }
        }

        /// <summary>
        /// The boolean flag that indicates whether or not
        /// the subscriber wants to cancel an operation that
        /// an event is related to.
        /// </summary>
        private bool _eventCancel;

        /// <summary>
        /// The EventCancel property provides access to the
        /// boolean indicator that can be set to cancel any
        /// operation that supports cancellation.
        /// </summary>
        public bool EventCancel
        {
            get { return _eventCancel; }
            set { _eventCancel = value; }
        }
    }
}
