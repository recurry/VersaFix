//************************************************************************
// The Vortex FIX Engine. Copyright (c) 2009 Assimilate Technology, Inc.
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.FixVxRegistry
{
    /// <summary>
    /// The IVfxFixVxRegistry interface defines an interface that
    /// can be implemented to provide the application with access
    /// to a collection of FIX version definitions. Note that the
    /// implementation of this interface is only done to preserve
    /// the semantics of the service container; there are not any
    /// other implementations apart from VfxFixVxRegistry...
    /// </summary>
    public interface IVfxFixVxRegistry : IEnumerable<VfxFixVxRecord>
    {
        /// <summary>
        /// The SelectRecord method is invoked to retrieve an
        /// instance of a specific version definition that has
        /// been previously added to the registry.
        /// </summary>
        /// <param name="name">
        /// The name of the version record to retrieve.
        /// </param>
        /// <returns>
        /// The corresponding instance of the version record
        /// that is in the registry, or null if not found.
        /// </returns>
        VfxFixVxRecord Get(string name);

        /// <summary>
        /// The RemoveRecord method is invoked to remove an
        /// instance of a specific version definition from the
        /// registry. The existing record is returned if there
        /// is an entry under the specified name, or the method
        /// returns null if there is no such entry.
        /// </summary>
        /// <param name="name">
        /// The name of the version definition that is to be
        /// removed from the registry.
        /// </param>
        /// <returns>
        /// The existing version definition, if one was found in
        /// the registry, or null if no record existed.
        /// </returns>
        VfxFixVxRecord Remove(string name);

        /// <summary>
        /// The InsertRecord method is invoked to insert an
        /// instance of a specific version definition into the
        /// registry, mapping it to the specified name.
        /// </summary>
        /// <param name="name">
        /// The name that the version definition is associated
        /// with in the registry.
        /// </param>
        /// <param name="record">
        /// The version definition that is to be added to the
        /// registry, keyed under the specified name.
        /// </param>
        /// <returns></returns>
        VfxFixVxRecord Add(string name, VfxFixVxRecord record);

    }

    /// <summary>
    /// The VfxFixVxRegistry (Vx=Versions) class provides an
    /// implementation of the IVfxFixVxService interface for
    /// managing a collection of version definitions.
    /// </summary>
    public class VfxFixVxRegistry : IVfxFixVxRegistry
    {
        // REC: Reader/Writer lock that protects the internal
        // map of version definitions:
        private ReaderWriterLock _rwLock = new ReaderWriterLock();

        // REC: The map of all FIX version definitions that have
        // been added to the version registry:
        private Dictionary<string, VfxFixVxRecord> _mapEntries = new Dictionary<string, VfxFixVxRecord>();

        public VfxFixVxRegistry()
        {

        }

        /// <summary>
        /// The Import method is invoked to load a collection
        /// of FIX version definitions from a file.
        /// </summary>
        /// <param name="path">
        /// The complete path to the file that contains all of
        /// the FIX version definitions that are to be loaded.
        /// </param>
        public void Import(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(VfxFixVxRecords));
                VfxFixVxRecords records = xs.Deserialize(fs) as VfxFixVxRecords;
                foreach (VfxFixVxRecord record in records.Elements)
                {
                    this.Add(record.Name, record);
                }
            }
        }

        #region IVfxFixVersions Members

        /// <summary>
        /// The EnumerateIndex method implementation provides for
        /// enumerating over a snapshot of all of the entries that
        /// are currently in the version catalog.
        /// </summary>
        /// <returns>
        /// An enumerator that allows the caller to enumerate the
        /// most recent snapshot of all the version entries.
        /// </returns>
        public IEnumerable<string> EnumerateIndex()
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            string[] vxNames = _mapEntries.Keys.ToArray();
            _rwLock.ReleaseReaderLock();

            foreach (string vxName in vxNames)
            {
                yield return vxName;
            }
        }

        public VfxFixVxRecord Get(string name)
        {
            VfxFixVxRecord result = null;

            if (!string.IsNullOrEmpty(name))
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                if (_mapEntries.ContainsKey(name))
                {
                    result = _mapEntries[name];
                }
                _rwLock.ReleaseReaderLock();
            }

            return result;
        }

        public VfxFixVxRecord Remove(string name)
        {
            throw new NotImplementedException();
        }

        public VfxFixVxRecord Add(string name, VfxFixVxRecord record)
        {
            VfxFixVxRecord result = record;

            _rwLock.AcquireWriterLock(Timeout.Infinite);

            if (!_mapEntries.ContainsKey(name))
            {
                _mapEntries.Add(name, record);
            }
            else
            {
                _mapEntries[name] = record;
            }

            _rwLock.ReleaseWriterLock();
            return record;
        }

        public IEnumerator<VfxFixVxRecord> GetEnumerator()
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            List<VfxFixVxRecord> listRelevant = new List<VfxFixVxRecord>();

            foreach (VfxFixVxRecord record in this._mapEntries.Values)
            {
                listRelevant.Add(record);
            }

            _rwLock.ReleaseReaderLock();

            foreach (VfxFixVxRecord record in listRelevant)
            {
                yield return record;
            }
        
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
