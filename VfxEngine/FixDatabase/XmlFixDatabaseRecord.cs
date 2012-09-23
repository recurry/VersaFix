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
using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.FixDatabase
{
    /// <summary>
    /// The XmlFixDatabaseEntry class provides a representation
    /// of a FIX session database entry that is compatible with
    /// the .NET XML serializer.
    /// </summary>
    [XmlRoot("Session")]
    public sealed class XmlFixDatabaseRecord
    {
        /// <summary>
        /// The next sequence number that the session should
        /// use when sending a message to its peer.
        /// </summary>
        [XmlElement("TxSequence")]
        public int TxSequence;

        /// <summary>
        /// The next sequence number that the session should 
        /// expect to receive from its peer.
        /// </summary>
        [XmlElement("RxSequence")]
        public int RxSequence;

        /// <summary>
        /// The FIX SenderCompID assigned to the session.
        /// </summary>
        [XmlElement("SenderCompID")]
        public string SenderCompID;

        /// <summary>
        /// The FIX TargetCompID assigned to the session.
        /// </summary>
        [XmlElement("TargetCompID")]
        public string TargetCompID;
    }

}
