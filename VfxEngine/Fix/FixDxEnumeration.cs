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
    /// The FixDxEnumeration (Dx=Dictionary) class encapsulates
    /// a set of enumerated values that can be associated with an
    /// instance of a field in the dictionary.
    /// </summary>
    public class FixDxEnumeration : IFixDxElement
    {
        private string _name;

        private Collection<FixDxEnumerator> _enumerators = new Collection<FixDxEnumerator>();

        /// <summary>
        /// The Enumerators property provides access to the
        /// collection of enumerators that are contained in
        /// an instance of an enumeration.
        /// </summary>
        public Collection<FixDxEnumerator> Enumerators
        {
            get { return _enumerators; }
        }

        /// <summary>
        /// Initializes a new instance of FixDxEnumeration with
        /// the specified parameters.
        /// </summary>
        /// <param name="name">
        /// The name to assign to the enumeration.
        /// </param>
        public FixDxEnumeration(string name)
        {
            _name = name;
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
        /// The HasTag property provides access to a boolean
        /// that indicates if the element has a FIX tag that
        /// is associated with it.
        /// </summary>
        public bool HasTag
        {
            get { return false;  }
        }

        /// <summary>
        /// The Name property provides access to the name
        /// that is assigned to an instance of an enumeration.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The Required property provides access to the flag
        /// that is used to determine if the element is a required
        /// component of an entity that it belongs to.
        /// </summary>
        public bool Required
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
