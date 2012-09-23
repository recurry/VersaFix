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
    /// The FixDxGroupReference class encapsulates the details
    /// of an instance of an inline group within the definition
    /// for a FIX message in the dictionary. Note that the use of
    /// the term "Reference," in this case, simply means that the
    /// group's name refers to a FIX tag in the field definitions
    /// section of the dictionary; the actual elements that belong
    /// to the repeating group ARE contained within the instance.
    /// </summary>
    public class FixDxGroupReference : IFixDxElement
    {
        // REC: The group's FIX name:
        private string _name;

        // REC: The group's required flag:
        private bool _required;

        // REC: The group's elements:
        private FixDxCollection _elements = new FixDxCollection();
        
        /// <summary>
        /// The Elements property provides access to the collection
        /// of FIX elements that are defined within an instance of 
        /// </summary>
        public FixDxCollection Elements
        {
            get { return _elements; }
        }

        /// <summary>
        /// Initializes an instance of FixDxGroupReference with the
        /// specified parameters.
        /// </summary>
        /// <param name="name">
        /// The name of the FIX repeating group that the reference
        /// is refering to.
        /// </param>
        public FixDxGroupReference(string name)
        {
            _name = name;
            _required = false;
        }

        /// <summary>
        /// Initializes an instance of FixDxGroupReference with the
        /// specified parameters.
        /// </summary>
        /// <param name="name">
        /// The name of the FIX repeating group that the reference
        /// is refering to.
        /// </param>
        /// <param name="required">
        /// Indicates whether or not the repeating group is a required
        /// component in a message that includes the reference.
        /// </param>
        public FixDxGroupReference(string name, bool required)
        {
            _name = name;
            _required = required;
        }

        #region IFixDxElement Members

        /// <summary>
        /// The Tag method retrieves the element's FIX tag
        /// or throws a NotSupportedException if the element
        /// implementation doesn't include a FIX tag.
        /// </summary>
        /// <returns>
        /// The FIX tag associated with the element, or the
        /// method throws a NotSupportedException.
        /// </returns>
        public int Tag
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The HasTag property is implemented to always return
        /// false since there is no FIX tag associated with the
        /// instance of a group reference.
        /// </summary>
        public bool HasTag
        {
            get { return false; }
        }

        /// <summary>
        /// The Name method retrieves the element's FIX name
        /// or throws a NotSupportedException if the element
        /// implementation doesn't include a FIX name.
        /// </summary>
        /// <returns>
        /// The FIX name associated with the element, or the
        /// method throws a NotSupportedException.
        /// </returns>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The Required property provides access to the boolean
        /// that indicates whether or not it is a required element
        /// in a FIX message. If the element implementation doesn't
        /// support this property, it throws NotSupportedException.
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

        #endregion
    }
}
