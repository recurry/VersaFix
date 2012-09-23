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

namespace VfxEngine.FixParser
{
    /// <summary>
    /// The VfxFixParserStatus enumeration defines the set of values
    /// that can be returned from an instance of the parser when the
    /// parsing operation has completed.
    /// </summary>
    public enum VfxFixParserStatus
    {
        
        /// <summary>
        /// REC: The MsgComplete code indicates that the
        /// message was successfully parsed from the buffer.
        /// </summary>
        MsgComplete = 0x01,
        /// <summary>
        /// REC: The MsgExhausted code indicates that there
        /// was not enough data in the buffer to support full
        /// parsing of a FIX message from it:
        /// </summary>
        MsgExhausted = 0x02,
        /// <summary>
        /// REC: The MsgIncomplete code indicates that the
        /// message was completely parsed but was incomplete
        /// according to the FIX protocol version:
        /// </summary>
        MsgIncomplete = 0x04,
        /// <summary>
        /// REC: The MsgMalformed code indicates that the
        /// message cannot be parsed properly:
        /// </summary>
        MsgMalformed = 0x08,
        /// <summary>
        /// REC: The MsgUnkSxProtocol code indicates that the
        /// message's session protocol version is unknown:
        /// </summary>
        MsgUnkSxProtocol = 0x10,
        /// <summary>
        /// REC: The MsgUnkAxProtocol code indicates that the
        /// message's application protocol version is unknown:
        /// </summary>
        MsgUnkAxProtocol = 0x20
    }
}
