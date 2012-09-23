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
    /// The FixDxResolvedField (Dx=Dictionary) class encapsulates
    /// all of the resolved information for a field definition in
    /// an instance of a FIX dictionary. 
    /// </summary>
    public sealed class FixDxResolvedField : IFixDxElement
    {
        // REC: The field's FIX tag:
        private int _tag;

        // REC: The field's FIX name:
        private string _name;

        // REC: The field's FIX type:
        private string _type;

        /// <summary>
        /// The Type property provides access to a string
        /// representation of the FIX data type that is 
        /// associated with a resolved field.
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        // REC: The field's required flag:
        private bool _required = false;

        // REC: The field's length encoding status:
        private bool _lengthCoded;

        /// <summary>
        /// The LengthCoded property provides access to a 
        /// flag that indicates whether or not the content
        /// of the field is length encoded.
        /// </summary>
        public bool LengthCoded
        {
            get { return _lengthCoded; }
        }

        // REC: the field that contains the length for
        // the content in an instance of this field:
        private int _lengthField;

        /// <summary>
        /// The LengthField property provides access to
        /// the FIX tag of the field that contains the
        /// length value for the content of this field.
        /// </summary>
        public int LengthField
        {
            get { return _lengthField; }
        }

        /// <summary>
        /// Initializes a new instance of FixDxResolvedField with
        /// the specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        public FixDxResolvedField(int tag, string name)
        {
            _tag = tag;
            _name = name;
            _type = "String";
            _required = false;
            _lengthCoded = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        /// <param name="required">
        /// The field's required status flag which indicates 
        /// whether or not the field is a required component
        /// of its parent element.
        /// </param>
        public FixDxResolvedField(int tag, string name, bool required)
        {
            _tag = tag;
            _name = name;
            _type = "String";
            _required = required;
            _lengthCoded = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        /// <param name="type">
        /// The field's FIX type.
        /// </param>
        public FixDxResolvedField(int tag, string name, string type)
        {
            _tag = tag;
            _name = name;
            _type = type;
            _lengthCoded = false;
            _required = false;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        /// <param name="type">
        /// The field's FIX type.
        /// </param>
        /// <param name="required">
        /// The fields "required" flag.
        /// </param>
        public FixDxResolvedField(int tag, string name, string type, bool required)
        {
            _tag = tag;
            _name = name;
            _type = type;
            _required = required;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        public FixDxResolvedField(int tag, string name, int length)
        {
            _tag = tag;
            _name = name;
            _type = "Data";
            _lengthCoded = true;
            _lengthField = length;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        public FixDxResolvedField(int tag, string name, int length, bool required)
        {
            _tag = tag;
            _name = name;
            _lengthCoded = true;
            _lengthField = length;
            _required = required;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        public FixDxResolvedField(int tag, string name, string type, int length)
        {
            _tag = tag;
            _name = name;
            _type = type;
            _lengthCoded = true;
            _lengthField = length;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        public FixDxResolvedField(int tag, string name, string type, int length, bool required)
        {
            _tag = tag;
            _name = name;
            _type = type;
            _lengthCoded = true;
            _lengthField = length;
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
        /// The HasTag property is implemented to always return
        /// false since there is no FIX tag that is associated with
        /// an instance of a component block definition.
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
            get { return _required;  }
        }

        #endregion
    }
}
