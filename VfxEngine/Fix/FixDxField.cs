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
    /// The FixDxField (Dx=Dictionary) class encapsulates 
    /// the details of a FIX field entry in a dictionary.
    /// 
    /// NOTE: Although this class provides accessors that
    /// can be used to modify its fields that are related
    /// to the IFixDxElement interface, these fields must
    /// not be modified if the instance is being stored in
    /// an instance of FixDxCollection - this would cause
    /// the element maps in the collection to be invalid.
    /// </summary>
    public sealed class FixDxField : IFixDxElement
    {
        // REC: The field's FIX tag:
        private int _tag;

        // REC: The field's FIX name:
        private string _name;

        // REC: The field's FIX data type:
        private string _type;

        /// <summary>
        /// The Type property provides access to a string
        /// representation of the field's FIX data type.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private bool _lengthCoded;

        /// <summary>
        /// The LengthCoded property provides access to a flag
        /// that indicates whether or not the field instance is
        /// a length encoded field. If this property is set to
        /// true, the LengthField property of the instance will
        /// also be set to the name of the field that should be
        /// reference in order to determine the length of this
        /// field's content.
        /// </summary>
        public bool LengthCoded
        {
            get { return _lengthCoded; }
            set { _lengthCoded = value; }
        }

        private string _lengthField;

        /// <summary>
        /// The LengthField property provides access to the
        /// name of the FIX field that contains the length of
        /// this field's content. This is used if the field is
        /// a length encoded field, and is only relevant if the
        /// field's LengthCoded property is set to true.
        /// </summary>
        public string LengthField
        {
            get { return _lengthField; }
            set { _lengthField = value; }
        }

        private string _enumeration;

        /// <summary>
        /// The Enumeration property provides access to the
        /// name of an enumeration that is associated with an
        /// instance of the field. The enumeration name can be
        /// used to retrieve the FixDxEnumeration instance that
        /// corresponds to it from the dictionary.
        /// </summary>
        public string Enumeration
        {
            get { return _enumeration; }
            set { _enumeration = value; }
        }

        /// <summary>
        /// Initializes a new instance of FixDxField with specific
        /// information related to the FIX field it represents.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        public FixDxField(int tag, string name)
        {
            _tag = tag;
            _name = name;
            _type = "String";
            _lengthCoded = false;
        }

        /// <summary>
        /// Initializes a new instance of FixDxField with specific
        /// information related to the FIX field it represents.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        /// <param name="type">
        /// The field's FIX data type.
        /// </param>
        public FixDxField(int tag, string name, string type)
        {
            _tag = tag;
            _name = name;
            _type = type;
            _lengthCoded = false;
        }

        /// <summary>
        /// Initializes a new instance of FixDxField with specific
        /// information related to the FIX field it represents.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="name">
        /// The field's FIX name.
        /// </param>
        /// <param name="type">
        /// The field's FIX data type.
        /// </param>
        /// <param name="lengthField">
        /// The name of the FIX field that contains the length
        /// of the content of this field's value. 
        /// </param>
        public FixDxField(int tag, string name, string type, string lengthField)
        {
            _tag = tag;
            _name = name;
            _type = type;
            _lengthCoded = true;
            _lengthField = lengthField;
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
        /// true since there is a FIX tag that is associated with
        /// an instance of a field.
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
            get { return _name; }
        }

        /// <summary>
        /// The Required property provides access to the boolean
        /// that indicates whether or not it is a required element
        /// in a FIX message. If the element implementation doesn't
        /// support this property, it throws NotSupportedException.
        /// </summary>
        public bool Required
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
