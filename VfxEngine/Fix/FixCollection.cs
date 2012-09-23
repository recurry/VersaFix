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
    /// The FixCollection class maintains a collection of elements
    /// that are components of a FIX message.
    /// </summary>
    public class FixCollection : IEnumerable<IFixElement>
    {
        // REC: The sequential list of all elements that
        // have been added to the collection:
        private Collection<IFixElement> _listElements = new Collection<IFixElement>();
        
        // REC: The map of all field elements that have
        // been added to the collection, keyed by their
        // respective FIX tags:
        private Dictionary<int, List<FixField>> _mapFields = new Dictionary<int, List<FixField>>();

        // REC: the map of all group elements that have
        // been added to the collection, keyed by their
        // respective FIX tags:
        private Dictionary<int, List<FixGroup>> _mapGroups = new Dictionary<int, List<FixGroup>>();

        // REC: The map of all the elements that are in
        // the collection, keyed by their FIX tags:
        private Dictionary<int, List<IFixElement>> _mapElements = new Dictionary<int, List<IFixElement>>();

        /// <summary>
        /// The SortEntry class is an internal class that is
        /// used to maintain the details of the sort ordering
        /// for each element that is in the ordering assigned
        /// to an instance of a collection.
        /// </summary>
        private class SortEntry
        {
            /// <summary>
            /// The element's FIX tag.
            /// </summary>
            public int Tag;
         
            /// <summary>
            /// The element's instance index.
            /// </summary>
            public int Index;

            /// <summary>
            /// Initializes an instance of the SortEntry class
            /// with the specified parameters.
            /// </summary>
            /// <param name="tag">
            /// The element's FIX tag.
            /// </param>
            /// <param name="index">
            /// The element's instance index.
            /// </param>
            public SortEntry(int tag, int index)
            {
                Tag = tag;
                Index = index;
            }
        }

        /// <summary>
        /// The list of SortEntry instances that define how the
        /// collection is sorted when it is enumerated.
        /// </summary>
        private List<SortEntry> _listOrdering = new List<SortEntry>();

        /// <summary>
        /// The AddField method adds an instance of a FIX field
        /// to the collection. Multiple fields with the same tag
        /// can be added to the collection, and will be stored in
        /// a list that is keyed by the field's FIX tag. 
        /// </summary>
        /// <param name="field">
        /// The FIX field to add to the collection.
        /// </param>
        public void AddField(FixField field)
        {
            // REC: Ensure that the field doesn't collide with
            // an existing group that has the same tag:
            if (!_mapGroups.ContainsKey(field.Tag))
            {
                if (!_mapFields.ContainsKey(field.Tag))
                {
                    _mapFields.Add(field.Tag, new List<FixField>());
                }

                _mapFields[field.Tag].Add(field);

                if (!_mapElements.ContainsKey(field.Tag))
                {
                    _mapElements.Add(field.Tag, new List<IFixElement>());
                }

                _mapElements[field.Tag].Add(field);

                // REC: Add to the sequential list:
                _listElements.Add(field);
            }
            else
            {
                throw new ArgumentException(string.Format("Field's tag collides with an existing group."));
            }
        }

        /// <summary>
        /// The SetField method attempts to assign a value to
        /// a field in the collection. If the field is already
        /// present within the collection, the existing value is
        /// replaced with the supplied value. If the field is not
        /// already in the collection, the field is added to it.
        /// </summary>
        /// <param name="field">
        /// The FIX field instance that is to be either assigned
        /// or added to the collection. If the operation results
        /// in an assigment, the Content property of the supplied
        /// field is copied into the corresponding property of the
        /// existing field, otherwise the entire field instance is
        /// added to the collection.
        /// </param>
        public void SetField(FixField field)
        {
            // REC: If the field is not already present in the
            // collection, the request can be forwarded to the
            // regular AddField method:
            if (!_mapFields.ContainsKey(field.Tag))
            {
                AddField(field);
            }
            else
            {
                // REC: Ensure that the field doesn't collide
                // with a group that is already present:
                if (!_mapGroups.ContainsKey(field.Tag))
                {
                    // REC: The content of the supplied field can
                    // be assigned directly to the content property
                    // of the existing field, this ensures that the
                    // field remains in the same location that it is
                    // in within the sequential list, as well as the
                    // map, of existing elements.
                    _mapFields[field.Tag][0].Content = field.Content;
                }
                else
                {
                    throw new ArgumentException("Field collides with an existing group.");
                }
            }
        }
        
        /// <summary>
        /// The GetField method returns the first instance of
        /// a field that matches the specified FIX tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the field to retrieve.
        /// </param>
        /// <returns>
        /// The first instance of a FIX field in the collection
        /// that matches the specified tag, or null if there is
        /// no matching entry.
        /// </returns>
        public FixField GetField(int tag)
        {
            FixField result = null;
            
            if (_mapFields.ContainsKey(tag))
            {
                result = _mapFields[tag][0];
            }
         
            return result;
        }

        /// <summary>
        /// The GetFields method returns the set of all fields
        /// in the collection that have the specified FIX tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the field(s) to be retrieved.
        /// </param>
        /// <returns>
        /// The set of all fields in the collection that have
        /// the specified tag, or null if there are no matches.
        /// </returns>
        public Collection<FixField> GetFields(int tag)
        {
            Collection<FixField> result = null;
            
            if (_mapFields.ContainsKey(tag))
            {
                result = new Collection<FixField>();
                foreach (FixField field in _mapFields[tag])
                {
                    result.Add(field);
                }
            }

            return result;
        }

        /// <summary>
        /// The AddGroup method adds an instance of a FIX group
        /// to the collection. Multiple fields with the same tag
        /// can be added to the collection, and will be stored in
        /// a list that is keyed by the field's FIX tag. 
        /// </summary>
        /// <param name="group">
        /// The FIX group to add to the collection.
        /// </param>
        public void AddGroup(FixGroup group)
        {
            // REC: Ensure that the group's tag doesn't collide
            // with a field that is already in the collection:
            if (!_mapFields.ContainsKey(group.Tag))
            {
                if (!_mapGroups.ContainsKey(group.Tag))
                {
                    _mapGroups.Add(group.Tag, new List<FixGroup>());
                }

                _mapGroups[group.Tag].Add(group);

                if (!_mapElements.ContainsKey(group.Tag))
                {
                    _mapElements.Add(group.Tag, new List<IFixElement>());
                }

                _mapElements[group.Tag].Add(group);

                // REC: Add the group to the sequential list of elements:
                _listElements.Add(group);

            }
            else
            {
                throw new ArgumentException("Group's tag collides with an existing field.");
            }
        
        }

        /// <summary>
        /// The GetGroup method returns the first instance of
        /// a FIX group that has the specified FIX tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the group to retrieve.
        /// </param>
        /// <returns>
        /// The first instance of a FIX group in the collection
        /// that has the specified tag.
        /// </returns>
        public FixGroup GetGroup(int tag)
        {
            FixGroup result = null;

            if (_mapGroups.ContainsKey(tag))
            {
                result = _mapGroups[tag][0];
            }

            return result;
        }

        /// <summary>
        /// The GetGroups method returns the set of all groups
        /// in the collection that have the specified FIX tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the group(s) to be retrieved.
        /// </param>
        /// <returns>
        /// The set of all groups in the collection that have
        /// the specified tag, or null if there are no matches.
        /// </returns>
        public Collection<FixGroup> GetGroups(int tag)
        {
            Collection<FixGroup> result = null;

            if (_mapGroups.ContainsKey(tag))
            {
                result = new Collection<FixGroup>();
                foreach (FixGroup group in _mapGroups[tag])
                {
                    result.Add(group);
                }
            }

            return result;
        }

        /// <summary>
        /// The GetElement method retrieves the first instance
        /// of a specific element in the collection.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the element to retrieve.
        /// </param>
        public IFixElement GetElement(int tag)
        {
            IFixElement result = null;

            if (_mapElements.ContainsKey(tag))
            {
                result = _mapElements[tag][0];
            }

            return result;
        }

        /// <summary>
        /// The GetElement method retrieves the specified instance
        /// of a specific element in the collection.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the element to retrieve.
        /// </param>
        /// <param name="instance">
        /// The instance index of the element to retrieve.
        /// </param>
        public IFixElement GetElement(int tag, int instance)
        {
            IFixElement result = null;
            if (_mapElements.ContainsKey(tag))
            {
                if (instance <= (_mapElements.Count - 1))
                {
                    result = _mapElements[tag][instance];
                }
            }

            return result;
        }

        /// <summary>
        /// The SetOrdering method assigns an ordering for
        /// specific fields in a collection. This ordering 
        /// is applied when the user enumerates the fields
        /// that are in the collection.
        /// </summary>
        /// <param name="ordering">
        /// A collection of integers that represent the 
        /// tags of the elements, in the order that they
        /// must be returned by enumeration methods.
        /// </param>
        public void SetOrdering(Collection<int> ordering)
        {
            Dictionary<int, int> mapIndices = new Dictionary<int, int>();
            foreach (int elementTag in ordering)
            {
                if (!mapIndices.ContainsKey(elementTag))
                {
                    mapIndices.Add(elementTag, 0);
                }
                else
                {
                    mapIndices[elementTag]++;
                }

                SortEntry sortEntry = new SortEntry(elementTag, mapIndices[elementTag]);
                _listOrdering.Add(sortEntry);
            }

        }        
        
        #region IEnumerable<IFixElement> Members

        /// <summary>
        /// The GetEnumerator method is used to enumerate all
        /// elements that are in the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that allows for the enumeration of
        /// all the elements in the collection.
        /// </returns>
        public IEnumerator<IFixElement> GetEnumerator()
        {
            if (_listOrdering.Count > 0)
            {
                Dictionary<int, int> mapCounters = new Dictionary<int, int>();
                foreach (SortEntry sortEntry in _listOrdering)
                {
                    if (!mapCounters.ContainsKey(sortEntry.Tag))
                    {
                        mapCounters.Add(sortEntry.Tag, 1);
                    }
                    else
                    {
                        mapCounters[sortEntry.Tag] = mapCounters[sortEntry.Tag] + 1;
                    }
                }

                List<IFixElement> listResidual = new List<IFixElement>();
                foreach (IFixElement element in _listElements)
                {
                    if (mapCounters.ContainsKey(element.Tag))
                    {
                        mapCounters[element.Tag] = mapCounters[element.Tag] - 1;
                        if (mapCounters[element.Tag] == 0)
                        {
                            mapCounters.Remove(element.Tag);
                        }
                    }
                    else
                    {
                        listResidual.Add(element);
                    }
                }

                List<IFixElement> elements = new List<IFixElement>();
                foreach (SortEntry sortEntry in _listOrdering)
                {
                    IFixElement element = GetElement(sortEntry.Tag, sortEntry.Index);
                    if (element != null)
                    {
                        elements.Add(element);
                    }
                }

                elements.Concat(listResidual);
                foreach(IFixElement element in elements)
                {
                    yield return element;
                }
            }
            else
            {
                foreach (IFixElement element in _listElements)
                {
                    yield return element;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// The GetEnumerator method is used to enumerate all
        /// elements that are in the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that allows for the enumeration of
        /// all the elements in the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
