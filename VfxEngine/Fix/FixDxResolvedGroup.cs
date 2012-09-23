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
    /// The FixDxResolvedGroup (Dx=Dictionary) class encapsulates
    /// all of the resolved information for a group definition in
    /// an instance of a FIX dictionary. 
    /// </summary>
    public sealed class FixDxResolvedGroup : IFixDxElement
    {
        // REC: The group's FIX tag:
        private int _tag;

        // REC: The group's FIX name:
        private string _name;

        // REC: The group's FIX type:
        private string _type;

        /// <summary>
        /// The Type property provides access to the string
        /// representation of the group's FIX data type.
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        // REC: The group's FIX required status:
        private bool _required;

        // REC: The collection of elements that the
        // group's instances are composed of:
        private FixDxCollection _elements = new FixDxCollection();

        /// <summary>
        /// The Elements property provides access to the
        /// collection of FIX elements that are contained
        /// within an instance of the repeating group.
        /// </summary>
        public FixDxCollection Elements
        {
            get { return _elements; }
        }

        /// <summary>
        /// Initializes an instance of FixDxResolvedGroup with
        /// the specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag for the repeating group.
        /// </param>
        /// <param name="name">
        /// The FIX name for the repeating group.
        /// </param>
        public FixDxResolvedGroup(int tag, string name)
        {
            _tag = tag;
            _name = name;
            _type = "NumInGroup";
            _required = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The group's FIX tag.
        /// </param>
        /// <param name="name">
        /// The group's FIX name.
        /// </param>
        /// <param name="required">
        /// The group's required status.
        /// </param>
        public FixDxResolvedGroup(int tag, string name, bool required)
        {
            _tag = tag;
            _name = name;
            _type = "NumInGroup";
            _required = required;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The group's FIX tag.
        /// </param>
        /// <param name="name">
        /// The group's FIX name.
        /// </param>
        /// <param name="type">
        /// The group's FIX type.
        /// </param>
        public FixDxResolvedGroup(int tag, string name, string type)
        {
            _tag = tag;
            _name = name;
            _type = "NumInGroup";
            _required = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The group's FIX tag.
        /// </param>
        /// <param name="name">
        /// The group's FIX name.
        /// </param>
        /// <param name="type">
        /// The group's FIX type.
        /// </param>
        /// <param name="required">
        /// The group's "required" flag.
        /// </param>
        public FixDxResolvedGroup(int tag, string name, string type, bool required)
        {
            _tag = tag;
            _name = name;
            _type = type;
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
            get { return _tag; }
        }

        /// <summary>
        /// The HasTag property provides access to a boolean
        /// that indicates if the element has a FIX tag that
        /// is associated with it.
        /// </summary>
        public bool HasTag
        {
            get { return true; }
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
            get { return _name;  }
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
        }

        #endregion
    }
}
