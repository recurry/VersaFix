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
    /// The IFixElement interface defines the interface that
    /// is common to elements of a FIX message. The accessor
    /// for the element value doesn't return a specific type
    /// since that can be handled by the implementing class.
    /// </summary>
    public interface IFixElement
    {
        /// <summary>
        /// The Tag property provides access to the FIX tag
        /// that is assigned to the element.
        /// </summary>
        int Tag { get; } 
        
        /// <summary>
        /// The Value property provides access to the value
        /// that is assigned to the element.
        /// </summary>
        string Content { get; set;  }
    }
}
