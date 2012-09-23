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
using System.Threading;

using VfxEngine.Fix;

namespace VfxEngine.FixDxRegistry
{
    /// <summary>
    /// The VfxFixDxRegistry (Dx=Dictionary) class provides an
    /// implementation of the IVfxFixDxRegistry interface for
    /// managing a collection of FIX dictionary instances. The
    /// VfxFixDxRegistry class is used throughout the VersaFix
    /// system for maintaining a central repository for all of
    /// the FIX dictionaries that the system is working with.
    /// </summary>
    public class VfxFixDxRegistry : IVfxFixDxRegistry
    {
        // REC: Reader/Writer lock that protects the internal
        // map of version definitions:
        private ReaderWriterLock _rwLock = new ReaderWriterLock();

        // REC: The map of all FIX dictionary definitions that 
        // have been added to the version registry:
        private Dictionary<string, FixDictionary> _mapEntries = new Dictionary<string, FixDictionary>();

        #region IVfxFixDxRegistry Members

        /// <summary>
        /// Creates a new entry in the registry for an instance
        /// of a FIX dictionary.
        /// </summary>
        /// <param name="name">
        /// The name that the dictionary is registered under.
        /// </param>
        /// <param name="dictionary">
        /// The reference to the dictionary being registered.
        /// </param>
        public void CreateEntry(string name, FixDictionary dictionary)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            if (!_mapEntries.ContainsKey(name))
            {
                _mapEntries.Add(name, dictionary);
            }
            else
            {
                _mapEntries[name] = dictionary;
            }
            _rwLock.ReleaseWriterLock();
                
        }

        /// <summary>
        /// Removes an existing entry for a specific FIX dictionary
        /// from the registry.
        /// </summary>
        /// <param name="name">
        /// The name that the dictionary is registered under.
        /// </param>
        public void RemoveEntry(string name)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            if (_mapEntries.ContainsKey(name))
            {
                _mapEntries.Remove(name);
            }
            _rwLock.ReleaseWriterLock();
        }

        /// <summary>
        /// Locates the FIX dictionary that is registered under
        /// the specified name and returns the reference to it.
        /// </summary>
        /// <param name="name">
        /// The name of the FIX dictionary to lookup.
        /// </param>
        /// <returns>
        /// A reference to the FIX dictionary that is registered
        /// under the specified name, or null if there is no entry
        /// that matches the specified name.
        /// </returns>
        public FixDictionary GetEntry(string name)
        {
            FixDictionary result = null;
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            if (_mapEntries.ContainsKey(name))
            {
                result = _mapEntries[name];
            }
            _rwLock.ReleaseReaderLock();
            return result;
        }

        #endregion


        #region IEnumerable<string> Members

        /// <summary>
        /// Returns an enumerator that can be used to enumerate
        /// all of the names of the dictionaries that have been
        /// registered. To protect the registry from concurrent
        /// read/write access, the method creates a snapshot of
        /// the current registry instead of enumerating against
        /// the live collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to enumerate all of
        /// the names of the dictionaries in the registry.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            string[] dxNames = _mapEntries.Keys.ToArray();
            _rwLock.ReleaseReaderLock();
            foreach (string dxName in dxNames)
            {
                yield return dxName;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that can be used to enumerate
        /// all of the names of the dictionaries that have been
        /// registered. To protect the registry from concurrent
        /// read/write access, the method creates a snapshot of
        /// the current registry instead of enumerating against
        /// the live collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to enumerate all of
        /// the names of the dictionaries in the registry.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
