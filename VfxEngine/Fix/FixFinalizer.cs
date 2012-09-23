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
    /// The FixFinalizer class finalizes an instance of
    /// a FIX message by calculating the correct values
    /// for common fields required by the FIX protocol.
    /// </summary>
    public sealed class FixFinalizer
    {
        /// <summary>
        /// The Finalize method calculates the values for
        /// common session layer fields and assigns those
        /// values to their respective fields.
        /// </summary>
        /// <param name="msg">
        /// The FIX message to be finalized.
        /// </param>
        public static void Finalize(FixMessage msg)
        {
            // REC: Calculate sending time and add it to the message:
            string strTimeStamp = DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss.fff");
            FixField fldSendingTime = msg.Header.GetField(52);
            if (fldSendingTime != null)
            {
                fldSendingTime.Content = strTimeStamp;
            }
            else
            {
                msg.Header.AddField(new FixField(52, strTimeStamp));
            }

            // REC: Calculate the body length and add it to the message:
            int fixBodyLength = FixCalculator.GetBodyLength(msg);
            FixField fldBodyLength = msg.Header.GetField(9);
            if (fldBodyLength != null)
            {
                fldBodyLength.Content = fixBodyLength.ToString();
            }
            else
            {
                msg.Header.AddField(new FixField(9, fixBodyLength.ToString()));
            }

            // REC: Calculate the checksum and add it to the message:
            string strChecksum = string.Format("{0:000}", FixCalculator.GetChecksum(msg) % 256);
            FixField fldChecksum = msg.Trailer.GetField(10);
            if (fldChecksum != null)
            {
                fldChecksum.Content = strChecksum;
            }
            else
            {
                msg.Trailer.AddField(new FixField(10, strChecksum));
            }

        }
    }
}
