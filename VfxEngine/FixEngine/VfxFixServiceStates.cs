using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServiceStates enumeration defines the list of
    /// all states that an instance of a FIX service implementation
    /// can be in at any given point in time.
    /// </summary>
    public enum VfxFixServiceStates
    {
        /// <summary>
        /// The Service_State_Opened state indicates that the service
        /// has been activated and is currently running.
        /// </summary>
        Service_State_Opened,
        /// <summary>
        /// The Service_State_Closing state indicates that the service
        /// has been asked to close and is currently shutting down.
        /// </summary>
        Service_State_Closing,
        /// <summary>
        /// The Service_State_Closed state indicates that the service
        /// has been shutdown / is no longer running.
        /// </summary>
        Service_State_Closed
    }
}
