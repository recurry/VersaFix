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
    /// The FixMessage class encapsulates the details of
    /// an instance of a FIX message.
    /// </summary>
    public class FixMessage
    {
        // REC: The collection of FIX elements that comprise
        // the header section of the message:
        private FixCollection _hdrElements = new FixCollection();

        /// <summary>
        /// The Header property provides access to the collection
        /// of FIX elements that are in the message's header.
        /// </summary>
        public FixCollection Header
        {
            get { return _hdrElements; }
        }

        // REC: The collection of FIX elements that comprise
        // the content section of the message:
        private FixCollection _msgElements = new FixCollection();

        /// <summary>
        /// The Content property provides access to the collection
        /// of FIX elements in the message's body.
        /// </summary>
        public FixCollection Content
        {
            get { return _msgElements; }
        }

        // REC: The collection if FIX elements that comprise
        // the trailer section of the message:
        private FixCollection _trlElements = new FixCollection();

        /// <summary>
        /// The Trailer property provides access to the collection
        /// of FIX elements in the trailer of a message.
        /// </summary>
        public FixCollection Trailer
        {
            get { return _trlElements; }
        }

        /// <summary>
        /// The AddField method adds an instance of a FIX field
        /// to the body of the message.
        /// </summary>
        /// <param name="field"></param>
        public void AddField(FixField field)
        {
            _msgElements.AddField(field);
        }


        /// <summary>
        /// The GetField method is invoked to retrieve the first
        /// instance of a FIX field that matches a specific tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the field to retrieve.
        /// </param>
        /// <returns>
        /// The FIX field that is associated with the specified
        /// tag, or null if there is no matching entry.
        /// </returns>
        public FixField GetField(int tag)
        {
            return _msgElements.GetField(tag);
        }


        /// <summary>
        /// The AddGroup method is invoked to add a new instance
        /// of a repeating group to an instance of a message.
        /// </summary>
        /// <param name="group">
        /// The repeating group to add to the message.
        /// </param>
        public void AddGroup(FixGroup group)
        {
            _msgElements.AddGroup(group);
        }

        /// <summary>
        /// The GetGroup method is invoked to retrieve the first
        /// instance of a repeating group that is associated with
        /// a specific FIX tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the group to retrieve.
        /// </param>
        /// <returns>
        /// The repeating group associated with the specified
        /// tag, or null if there is no matching entry.
        /// </returns>
        public FixGroup GetGroup(int tag)
        {
            return _msgElements.GetGroup(tag);
        }

        /// <summary>
        /// The ToString method overrides the base class method
        /// to provide a string representation of the message.
        /// </summary>
        /// <returns>
        /// The string representation of the FIX message.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            // REC: Process all of the header elements:
            sb.Append(ConvertElements(_hdrElements));
            sb.Append(ConvertElements(_msgElements));
            sb.Append(ConvertElements(_trlElements));
            return sb.ToString();
        }

        /// <summary>
        /// The ConvertElements method converts all of the elements
        /// in an instance of FixCollection into their corresponding
        /// representation as a text string. This is a helper method
        /// for the FixMessage class implementation of ToString().
        /// </summary>
        /// <param name="elements">
        /// The collection of elements that are to be converted into
        /// their corresponding representation as a string.
        /// </param>
        /// <returns>
        /// The string that results from the conversion.
        /// </returns>
        private string ConvertElements(FixCollection elements)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IFixElement element in elements)
            {
                sb.Append(string.Format("{0}={1}\x01", element.Tag, element.Content));
                FixGroup group = element as FixGroup;
                if (group != null)
                {
                    foreach (FixCollection instance in group.Instances)
                    {
                        sb.Append(ConvertElements(instance));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
