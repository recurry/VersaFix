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
    /// The FixDxBlockReference class encapsulates the details
    /// of a block reference within a FIX dictionary. The block
    /// references are typically used in message definitions.
    /// </summary>
    public sealed class FixDxBlockReference : IFixDxElement
    {
        // REC: The name of the FIX component block that
        // the element refers to:
        private string _name;

        // REC: Indicates whether or not the component
        // block is a required component of a FIX message:
        private bool _required;

        #region IFixDxElement Members

        /// <summary>
        /// The Tag property is not implemented for instances
        /// of a component block reference since they use the
        /// name of the block rather than the tag.
        /// </summary>
        public int Tag
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The HasTag method is implemented to always return
        /// false since there is no FIX tag associated with an
        /// instance of a block reference.
        /// </summary>
        public bool HasTag
        {
            get { return false; }
        }


        /// <summary>
        /// The Name property provides access to the FIX name
        /// of the component block, as it is defined int the
        /// dictionary it is contained in.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The Required property indicates whether or not
        /// the component block is a required component of
        /// a message it is included in.
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
        public FixDxBlockReference()
        {
            _name = null;
            _required = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        /// <param name="name">
        /// The FIX block being referenced.
        /// </param>
        /// <param name="required">
        /// The reference's required status which indicates
        /// whether or not the block is a required component
        /// of the entity it is found in.
        /// </param>
        public FixDxBlockReference(string name, bool required)
        {
            _name = name;
            _required = required;
        }

        #endregion
    }
}
