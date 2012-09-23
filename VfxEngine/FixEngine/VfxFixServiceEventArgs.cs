using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServiceEventArgs class encapsulates the details
    /// of an event that is dispatched from a service instance.
    /// </summary>
    public class VfxFixServiceEventArgs : EventArgs
    {
        private VfxFixServiceEventTypes _eventType;

        /// <summary>
        /// The EventType property provides access to the type
        /// of the event that is being dispatched.
        /// </summary>
        public VfxFixServiceEventTypes EventType
        {
            get { return _eventType; }
        }

        private VfxFixServiceStatus _eventStatus;

        /// <summary>
        /// The EventStatus property provides access to the status
        /// associated with the event being dispatched.
        /// </summary>
        public VfxFixServiceStatus EventStatus
        {
            get { return _eventStatus; }
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// supplied parameters.
        /// </summary>
        /// <param name="eventType">
        /// The type of the event being dispatched.
        /// </param>
        /// <param name="eventStatus">
        /// The status of the service dispatching the event.
        /// </param>
        public VfxFixServiceEventArgs(VfxFixServiceEventTypes eventType, VfxFixServiceStatus eventStatus)
        {
            _eventType = eventType;
            _eventStatus = eventStatus;
        }
    }
}
