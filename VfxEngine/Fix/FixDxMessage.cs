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
    /// The FixDxMsgEntry class encapsulates the details
    /// of a message definition in the FIX dictionary.
    /// 
    /// NOTE: Although this class provides accessors that
    /// can be used to modify its fields that are related
    /// to the IFixDxElement interface, these fields must
    /// not be modified if the instance is being stored in
    /// an instance of FixDxCollection - this would cause
    /// the element maps in the collection to be invalid.
    /// </summary>
    public sealed class FixDxMessage : IFixDxElement
    {
        // REC: The message's FIX MsgType field:
        private string _msgType;

        /// <summary>
        /// The MsgType property provides access to the string
        /// representation of the message's FIX type.
        /// </summary>
        public string MsgType
        {
            get { return _msgType; }
            set { _msgType = value; }
        }

        // REC: The element's FIX name:
        private string _msgName;

        // REC: The message's category:
        private string _msgCategory;

        /// <summary>
        /// The MsgCategory property provides access to the
        /// category that is assigned to a message definition
        /// in a FIX dictionary.
        /// </summary>
        public string MsgCategory
        {
            get { return _msgCategory; }
            set { _msgCategory = value; }
        }

        // REC: The collection of elements that are
        // members of the message definition:
        private FixDxCollection _elements = new FixDxCollection();

        /// <summary>
        /// The Elements property provides access to the
        /// collection of FIX elements that are defined as
        /// the content of the message.
        /// </summary>
        public FixDxCollection Elements
        {
            get { return _elements; }
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
        /// false since there is no FIX tag that is associated with
        /// an instance of a message definition.
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
            get { return _msgName; }
            set { _msgName = value; }
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

        /// <summary>
        /// Initializes a new instance of the class with the
        /// appropriate default settings.
        /// </summary>
        public FixDxMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        /// <param name="msgType">
        /// The message's FIX message type.
        /// </param>
        /// <param name="msgName">
        /// The message's FIX message name.
        /// </param>
        public FixDxMessage(string msgType, string msgName)
        {
            _msgType = msgType;
            _msgName = msgName;
        }

        /// <summary>
        /// Initializes a new instance of the class with
        /// the specified parameters.
        /// </summary>
        /// <param name="msgType">
        /// The message's FIX message type.
        /// </param>
        /// <param name="msgName">
        /// The message's FIX message name.
        /// </param>
        /// <param name="msgCategory">
        /// The message's FIX message category.
        /// </param>
        public FixDxMessage(string msgType, string msgName, string msgCategory)
        {
            _msgType = msgType;
            _msgName = msgName;
            _msgCategory = msgCategory;
        }
        #endregion
    }
}
