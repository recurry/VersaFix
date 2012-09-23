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
    /// The FixDxBlockTypes enumeration defines the set of
    /// values which are used for indicating the type of a
    /// component block in a FIX dictionary.
    /// </summary>
    public enum FixDxBlockTypes
    {
        /// <summary>
        /// The type Component is used to indicate that the
        /// component block is a regular component block.
        /// </summary>
        Component,
        /// <summary>
        /// The type Repeating is used to indicate that the
        /// component block is used as a repeating group.
        /// </summary>
        Repeating
    }

    /// <summary>
    /// The FixDxBlock (Dx=Dictionary) class encapsulates the
    /// details for a component block in a FIX dictionary.
    /// </summary>
    public sealed class FixDxBlock : IFixDxElement
    {
        private string _name;

        private FixDxBlockTypes _type;

        /// <summary>
        /// The Type property returns the type of the component
        /// block. This can be any of the enumerated values that
        /// are defined in the FixDxBlockTypes enumeration.
        /// </summary>
        public FixDxBlockTypes Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _category;

        /// <summary>
        /// The Category property provides access to a string
        /// that denotes the block's category type.
        /// </summary>
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        private string _field;

        /// <summary>
        /// The Field property provides access to the name of
        /// the FIX field that marks the start of a repeating
        /// component block; this value is only relevant if the
        /// block represents a repeating block or group.
        /// </summary>
        public string Field
        {
            get { return _field; }
            set { _field = value; }
        }

        // REC: The collection of elements that are members
        // of the component block:
        private FixDxCollection _elements = new FixDxCollection();

        /// <summary>
        /// The Elements property provides access to the collection
        /// of FIX elements that the block is composed of.
        /// </summary>
        public FixDxCollection Elements
        {
            get { return _elements; }
        }

        /// <summary>
        /// This implementation of the FixDxBlock constructor
        /// is used to create an instance of a component block
        /// with the name field set. The component block's type
        /// is automatically set to FixDxBlockTypes.Component.
        /// </summary>
        /// <param name="name"></param>
        public FixDxBlock(string name)
        {
            _name = name;
            _type = FixDxBlockTypes.Component;
        }

        /// <summary>
        /// This implementation of the FixDxBlock constructor
        /// is used to create an instance of a component block
        /// with the name and category fields set. The component
        /// block's type will be automatically set to the value
        /// of FixDxBlockTypes.Component.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        public FixDxBlock(string name, string category)
        {
            _name = name;
            _type = FixDxBlockTypes.Component;
            _category = category;
        }

        /// <summary>
        /// This implementation of the FixDxBlock constructor
        /// is used to create an instance of a component block
        /// that represents a repeating group. 
        /// </summary>
        /// <param name="name">
        /// The component block's name.
        /// </param>
        /// <param name="category">
        /// The component block's category.
        /// </param>
        /// <param name="field">
        /// The name of the FIX field that is used as the
        /// count/start field for the repeating group.
        /// </param>
        public FixDxBlock(string name, string category, string field)
        {
            _name = name;
            _type = FixDxBlockTypes.Repeating;
            _category = category;
            _field = field;
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
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// The HasTag property is implemented to always return
        /// false since there is no FIX tag that is associated with
        /// an instance of a component block definition.
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
            get { throw new NotSupportedException(); }
        }

        #endregion

        

     
    }
}
