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

namespace VfxEngine.FixDxRegistry
{
    /// <summary>
    /// The IVfxFixDxRegistry defines an interface for a class
    /// that maintains a collection of FIX dictionaries, keyed
    /// by their respective dictionary names. The interface is
    /// intended to be used in situations where it may be read
    /// and written to by multiple threads, so implementations
    /// must take this into consideration by ensuring that the
    /// enumeration methods return an enumerator that iterates
    /// over a snapshot rather than the live collection.
    /// </summary>
    public interface IVfxFixDxRegistry : IEnumerable<string>
    {
        /// <summary>
        /// The CreateEntry method creates an entry in the
        /// registry for the supplied version definition.
        /// </summary>
        /// <param name="name">
        /// The name that the FIX dictionary is registered
        /// under in the directory.
        /// </param>
        /// <param name="entry">
        /// The FIX dictionary that is being added.
        /// </param>
        void CreateEntry(string name, FixDictionary entry);

        /// <summary>
        /// The RemoveEntry method removes the specified
        /// dictionary entry from the registry.
        /// </summary>
        /// <param name="name">
        /// The name of the dictionary entry that is to
        /// be removed from the registry.
        /// </param>
        void RemoveEntry(string name);

        /// <summary>
        /// The GetEntry method retrieves the dictionary
        /// definition associated with the supplied name.
        /// </summary>
        /// <param name="name">
        /// The name of the version definition to retrieve.
        /// </param>
        /// <returns>
        /// The instance of a version definition that is
        /// associated with the supplied name.
        /// </returns>
        FixDictionary GetEntry(string name);
    }
}
