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
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixApp
{
    /// <summary>
    /// The IVfxFixAppSession interface defines an interface
    /// for a FIX session that is passed to implementations of
    /// the IVfxFixApp class as one of the parameters to their
    /// callback methods. Implementations of IVfxFixApp can use
    /// this session interface to interact with the session that
    /// a specific callback relates to.
    /// </summary>
    public interface IVfxFixAppSession
    {
        /// <summary>
        /// The InstanceId property provides access to the unique
        /// identifier assigned to an instance of a session.
        /// </summary>
        System.Guid InstanceId { get; }

        /// <summary>
        /// The Settings property provides access to the settings that
        /// are associated with an instance of a session.
        /// </summary>
        //IVfxSettings Settings { get; }

        /// <summary>
        /// The Services property provides access to the services that
        /// are associated with an instance of a session.
        /// </summary>
        //IVfxServices Services { get; }
        
        /// <summary>
        /// The Send method sends an instance of a FIX message
        /// to the peer system that is connected to a session.
        /// </summary>
        /// <param name="msg">
        /// The FIX message to be sent to the peer session.
        /// </param>
        void Send(FixMessage msg);

    }
}
