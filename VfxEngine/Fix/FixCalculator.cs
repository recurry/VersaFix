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
    /// The FixCalculator class provides some utility methods that
    /// can be used to calculate the values of commmon FIX fields.
    /// </summary>
    public sealed class FixCalculator
    {
        /// <summary>
        /// The GetChecksum method calculates the checksum for
        /// an instance of a FIX message.
        /// </summary>
        /// <param name="msg">
        /// The FIX message to analyze.
        /// </param>
        /// <returns>
        /// The FIX protocol checksum of the message.
        /// </returns>
        public static int GetChecksum(FixMessage msg)
        {
            int result = 0;

            // REC: Process the header elements:
            foreach (IFixElement hdrElement in msg.Header)
            {
                result += CalculateSum(hdrElement.Tag.ToString());
                result += 0x3D;
                result += CalculateSum(hdrElement.Content);
                result += 0x01;

                FixGroup group = hdrElement as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateSum(instance);
                    }
                }
            }

            // REC: Process the message body:
            foreach (IFixElement msgElement in msg.Content)
            {
                result += CalculateSum(msgElement.Tag.ToString());
                result += 0x3D;
                result += CalculateSum(msgElement.Content);
                result += 0x01;

                FixGroup group = msgElement as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateSum(instance);
                    }
                }
            }

            // REC: Process the message trailer:
            foreach (IFixElement trlElement in msg.Trailer)
            {
                if (trlElement.Tag == 10)
                {
                    break;
                }
                
                result += CalculateSum(trlElement.Tag.ToString());
                result += 0x3D;
                result += CalculateSum(trlElement.Content);
                result += 0x01;

                FixGroup group = trlElement as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateSum(instance);
                    }
                }
            }

            return result; 
        }

        private static int CalculateSum(FixCollection elements)
        {
            int result = 0;

            foreach (IFixElement element in elements)
            {
                result += CalculateSum(element.Tag.ToString());
                result += 0x3D;
                result += CalculateSum(element.Content);
                result += 0x01;

                FixGroup group = element as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateSum(instance);
                    }
                }
            }

            return result;
        }

        private static int CalculateSum(string str)
        {
            int result = 0;

            for (int i = 0; i != str.Length; i++)
            {
                result += str[i];
            }

            return result;
        }


        /// <summary>
        /// The GetBodyLength method calculates the appropriate
        /// value for the FIX BodyLength field of the specified
        /// message, in accordance with the FIX Protocol.
        /// </summary>
        /// <param name="msg">
        /// The message that the BodyLength value is to be
        /// calculated from.
        /// </param>
        /// <returns>
        /// The body length of the message, calculated according
        /// to the method specified in the FIX protocol.
        /// </returns>
        public static int GetBodyLength(FixMessage msg)
        {
            int result = 0;

            // REC: Calculate the combined length of all
            // elements in the header of the message:
            bool lengthFound = false;
            foreach (IFixElement element in msg.Header)
            {
                if (lengthFound == true)
                {
                    result += element.Tag.ToString().Length;
                    result += 1;
                    result += element.Content.Length;
                    result += 1;

                    FixGroup group = element as FixGroup;
                    if (group != null)
                    {
                        foreach (FixCollection instance in group.Instances)
                        {
                            result += CalculateLength(instance);
                        }
                    }
                }
                else
                {
                    if (element.Tag == 9)
                    {
                        lengthFound = true;
                    }
                }
            }

            // REC: Calculate the combined length of all
            // the body elements in the message:
            foreach (IFixElement element in msg.Content)
            {
                result += element.Tag.ToString().Length;
                result += 1;
                result += element.Content.Length;
                result += 1;

                FixGroup group = element as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateLength(instance);
                    }
                }
            }

            // REC: Calculate the combined length of all 
            // elements in the trailer of the message:
            foreach (IFixElement element in msg.Trailer)
            {
                // REC: Terminate when the checksum field
                // is encountered in the trailer:
                if (element.Tag == 10)
                {
                    break;
                }
                
                result += element.Tag.ToString().Length;
                result += 1;
                result += element.Content.Length;
                result += 1;

                FixGroup group = element as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateLength(instance);
                    }
                }
            }

            return result;
        }

        // REC: The CalculateLength method calculates the combined
        // length of all of the elements in a collection. This is
        // a support method for the CalculateBodyLength method.
        private static int CalculateLength(FixCollection collection)
        {
            int result = 0;

            foreach (IFixElement element in collection)
            {
                result += element.Tag.ToString().Length;
                result += 1;
                result += element.Content.Length;
                result += 1;

                FixGroup group = element as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        result += CalculateLength(instance);
                    }
                }
            }

            return result;
        }
        

    }
}
