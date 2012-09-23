using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxTcpConnectorEventTypes enumeration defines the
    /// set of event types that can be dispatched by an instance
    /// of the VfxTcpConnector class.
    /// </summary>
    public enum VfxTcpConnectorEventTypes
    {
        /// <summary>
        /// The Event_Connect_Success event indicates that a
        /// connection attempt was successful.
        /// </summary>
        Event_Connect_Success,
        /// <summary>
        /// The Event_Connect_Failure event indicates that a
        /// connection attempt was not successful.
        /// </summary>
        Event_Connect_Failure
    }
}
