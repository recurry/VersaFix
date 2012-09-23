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
    /// The FixField class encapsulates the details of
    /// a field element in a FIX message.
    /// </summary>
    public class FixField : IFixElement
    {
        private int _tag;

        private string _value;
        
        /// <summary>
        /// Initializes an instance of the FixField class with
        /// the specified parameters.
        /// </summary>
        /// <param name="tag">
        /// The field's FIX tag.
        /// </param>
        /// <param name="value">
        /// The field's value/content.
        /// </param>
        public FixField(int tag, string value)
        {
            _tag = tag;
            _value = value;
        }

        #region IFixElement Members

        /// <summary>
        /// The Tag property provides access to the FIX tag
        /// that is assigned to the field.
        /// </summary>
        public int Tag
        {
            get { return _tag;  }
        }

        /// <summary>
        /// The Value property provides access to the value
        /// that is assigned to the field.
        /// </summary>
        public string Content
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion
    }
}
