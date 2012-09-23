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

namespace VfxEngine.FixSessions
{
    /// <summary>
    /// The FixSessionStates enumeration provides a list of
    /// states that an instance of a FIX session can be in.
    /// </summary>
    public enum FixSessionStates
    {
        /// <summary>
        /// The session is active, but the peer system has
        /// not yet logged on to the session.
        /// </summary>
        Session_Active,
        /// <summary>
        /// The session is active and the peer system has
        /// been logged on to the session.
        /// </summary>
        Session_Opened,
        /// <summary>
        /// The session is active but is closing the session
        /// with the peer system.
        /// </summary>
        Session_Closing,
        /// <summary>
        /// The session is active but the session with the
        /// peer system has been closed / logged out.
        /// </summary>
        Session_Closed,
        /// <summary>
        /// The session is shutdown and cannot exchange any
        /// messages with the peer system.
        /// </summary>
        Session_Shutdown
    }
}
