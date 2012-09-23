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

namespace VfxEngine.Fix
{
    /// <summary>
    /// The FixDxEnumerator (Dx=Dictionary) class contains the
    /// value and description for a single member of an enumeration
    /// that is assigned to a FIX field in the dictionary.
    /// </summary>
    public sealed class FixDxEnumerator
    {
        private string _value;

        /// <summary>
        /// The value that is associated with an instance
        /// of an enumerator.
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        private string _description;

        /// <summary>
        /// The description that is associated with an
        /// instance of an enumerator.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Initializes a new instance of FixDxEnumerator with
        /// the specified parameters.
        /// </summary>
        /// <param name="value">
        /// The value to assign to the enumerator.
        /// </param>
        public FixDxEnumerator(string value)
        {
            _value = value;
            _description = null;
        }

        /// <summary>
        /// Initializes a new instance of FixDxEnumerator with
        /// the specified parameters.
        /// </summary>
        /// <param name="value">
        /// The enumerator's value.
        /// </param>
        /// <param name="description">
        /// The enumerator's description.
        /// </param>
        public FixDxEnumerator(string value, string description)
        {
            _value = value;
            _description = description;
        }
    }
}
