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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The FixGroup class encapsulates the details of
    /// a repeating group element in a FIX message.
    /// </summary>
    public class FixGroup : FixField
    {
        // REC: The repeating group's instances:
        private Collection<FixCollection> _instances = new Collection<FixCollection>();

        /// <summary>
        /// The Instances property provides access to the
        /// collection of repeating group instances that are
        /// contained by the group.
        /// </summary>
        public Collection<FixCollection> Instances
        {
            get { return _instances; }
        }

        /// <summary>
        /// The FixGroup constructor just forwards to the
        /// underlying FixField's constructor.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag assigned to the group.
        /// </param>
        /// <param name="value">
        /// The value assigned to the group. This should
        /// usually be a string representation of an int
        /// that indicates how many repeating groups are
        /// contained within the group instance.
        /// </param>
        public FixGroup(int tag, string value) : base(tag, value)
        {

        }
    }
}
