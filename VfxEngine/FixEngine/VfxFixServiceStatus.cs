using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServiceStatus enumeration defines all of the
    /// values that can be used to express a service's status.
    /// </summary>
    public enum VfxFixServiceStatus
    {
        /// <summary>
        /// The Service_Status_Opened status indicates that the
        /// service has been started and is now running.
        /// </summary>
        Service_Status_Opened,

        /// <summary>
        /// The Service_Status_Connecting status indicates that the
        /// service is now attempting to connect to a peer system.
        /// </summary>
        Service_Status_Connecting,

        /// <summary>
        /// The Service_Status_Disconnected status indicates that the
        /// service has disconnected from its peer system.
        /// </summary>
        Service_Status_Disconnected,

        /// <summary>
        /// The Service_Status_Sleeping status indicates that the 
        /// service is waiting for a timed event to occur before it
        /// does anything else.
        /// </summary>
        Service_Status_Sleeping,

        /// <summary>
        /// The Service_Status_Closing status indicates that the
        /// service is in the process of shutting itself down.
        /// </summary>
        Service_Status_Closing,

        /// <summary>
        /// The Service_Status_Closed status type indicates that 
        /// the service has been shutdown and is no longer active.
        /// </summary>
        Service_Status_Closed,
    }
}
