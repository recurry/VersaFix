using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServiceEventTypes enumeration defines all of the
    /// possible event types that can be dispatched by a service.
    /// </summary>
    public enum VfxFixServiceEventTypes
    {
        /// <summary>
        /// The Event_Service_Started event is dispatched to notify
        /// the service's subscribers that it has been started.
        /// </summary>
        Event_Service_Started,

        /// <summary>
        /// The Event_Service_Updated event is dispatched to notify
        /// the service's subscribers of a status update.
        /// </summary>
        Event_Service_Updated,

        /// <summary>
        /// The Event_Service_Stopped event is dispatched to notify
        /// the service's subscribers that it has been shutdown.
        /// </summary>
        Event_Service_Stopped
    }
}
