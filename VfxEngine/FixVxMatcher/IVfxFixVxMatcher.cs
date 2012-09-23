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
using VfxEngine.FixVxRegistry;

namespace VfxEngine.FixVxMatcher
{
    /// <summary>
    /// The IVfxFixVxMatcher (Vx=Version) interface defines an
    /// interface for a component that analyzes a FIX message 
    /// to determine the FIX version that it relates to.
    /// </summary>
    public interface IVfxFixVxMatcher
    {
        /// <summary>
        /// The Init method initializes the version matcher
        /// with a reference to the version registry that it
        /// should use for analyzing messages.
        /// </summary>
        /// <param name="registry"></param>
        void Init(IVfxFixVxRegistry registry);

        /// <summary>
        /// The GetSxVersion method attempts to retrieve the
        /// name of the session layer protocol version which 
        /// applies to a collection of FIX elements.
        /// </summary>
        /// <param name="elements">
        /// The collection of FIX elements to analyze.
        /// </param>
        /// <returns>
        /// The name of the FIX protocol version that is the
        /// best match for the supplied elements.
        /// </returns>
        string GetSxVersion(FixCollection elements);

        /// <summary>
        /// The GetAxVersion method attempts to retrieve the
        /// name of the app layer protocol version which best 
        /// applies to a collection of FIX elements.
        /// </summary>
        /// <param name="elements">
        /// The collection of FIX elements to analyze.
        /// </param>
        /// <returns>
        /// The name of the FIX protocol version that is the
        /// best match for the supplied elements.
        /// </returns>
        string GetAxVersion(FixCollection elements);

        /// <summary>
        /// The GetVersion method attempts to retrieve the
        /// name of the protocol version, at the specified
        /// layer, that matches the specified elements.
        /// </summary>
        /// <param name="elements">
        /// The FIX message elements to evaluate.
        /// </param>
        /// <param name="layer">
        /// The layer of the application protocol that is
        /// being requested.
        /// </param>
        /// <returns>
        /// The name of the protocol version that matches
        /// the supplied elements, or null if none match.
        /// </returns>
        string GetVersion(FixCollection elements, string layer);


    }
}
