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

namespace VfxEngine.FixDatabase
{
    /// <summary>
    /// The VfxFixDatabaseEntry class encapsulates the details
    /// of a session that has been persisted to a FIX database
    /// implementation. This class is used to both retrieve and
    /// update session information from/to the database.
    /// </summary>
    public class VfxFixDatabaseRecord
    {
        /// <summary>
        /// The next sequence number that should be used when
        /// the session sends an outgoing message.
        /// </summary>
        private int _txSequence;

        /// <summary>
        /// The TxSequence property provides access to the next
        /// sequence number that should be used for a message that
        /// is being sent to the peer side of a FIX session.
        /// </summary>
        public int TxSequence
        {
            get { return _txSequence; }
            set { _txSequence = value; }
        }

        // REC: The next expected incoming sequence number.
        private int _rxSequence;

        /// <summary>
        /// The RxSequence property provides access to the next 
        /// sequence number that is expected to be received from
        /// the peer side of a FIX session.
        /// </summary>
        public int RxSequence
        {
            get { return _rxSequence; }
            set { _rxSequence = value; }
        }



    }
}
