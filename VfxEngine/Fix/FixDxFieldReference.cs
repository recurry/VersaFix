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
    /// The FixDxFieldReference class encapsulates the details
    /// of a field reference in a FIX dictionary.
    /// </summary>
    public sealed class FixDxFieldReference : IFixDxElement
    {
        // REC: The name of the FIX dictionary field that
        // the element refers to:
        private string _name;

        // REC: The required status of the reference which
        // indicates whether or not it is a required element
        // in an instance of a FIX message.
        private bool _required;

        #region IFixDxElement Members

        /// <summary>
        /// The Tag property is not implemented since field
        /// references refer to the name of the field in the
        /// dictionary rather than the FIX tag.
        /// </summary>
        public int Tag
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The HasTag property provides access to a boolean
        /// that indicates if the element has a FIX tag that
        /// is associated with it.
        /// </summary>
        public bool HasTag
        {
            get { return false; }
        }

        /// <summary>
        /// The Name property provides access to the FIX name
        /// of the field as it is defined in the dictionary it
        /// is contained in.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The Required property indicates whether or not the
        /// field reference is a required component of a message.
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// appropriate default settings.
        /// </summary>
        public FixDxFieldReference()
        {
            _name = null;
            _required = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="name">
        /// The FIX field being referenced.
        /// </param>
        /// <param name="required">
        /// The field reference's required flag which indicates
        /// whether or not the field is required.
        /// </param>
        public FixDxFieldReference(string name, bool required)
        {
            _name = name;
            _required = required;
        }

        #endregion
    }
}
