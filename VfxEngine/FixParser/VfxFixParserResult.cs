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

using VfxEngine.Fix;

namespace VfxEngine.FixParser
{
    /// <summary>
    /// The VfxFixParserResult class encapsulates the result details
    /// that are returned by the VfxFixParser class when it attempts 
    /// to parse a message from a buffer.
    /// </summary>
    public class VfxFixParserResult
    {
        /// <summary>
        /// The Consumed property provides access to the number
        /// of bytes that were consumed by the parser during the
        /// parsing operation the result relates to.
        /// </summary>
        public int Consumed = 0;

        /// <summary>
        /// The Message property provides access to the instance
        /// of a FIX message that the parser has extracted from the
        /// data that was supplied to it.
        /// </summary>
        public FixMessage Message = new FixMessage();

        /// <summary>
        /// The Status property provides access to the status
        /// indicator for the parsing operation that the result
        /// is related to.
        /// </summary>
        public VfxFixParserStatus Status = VfxFixParserStatus.MsgComplete;
    }
}
