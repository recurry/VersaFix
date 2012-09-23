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
    /// The VfxFixMatcher class provides a concrete implementation
    /// of the IVfxFixMatcher interface.
    /// </summary>
    public class VfxFixVxMatcher : IVfxFixVxMatcher
    {
        /// <summary>
        /// The MatchCriteria class is an internal class that is
        /// used to encapsulate the version matching criteria for
        /// each of the versions in the version registry.
        /// </summary>
        private class MatchEntry
        {
            // REC: The name of the version that is related
            // to the list of version match criteria:
            public string Name = null;

            // REC: The criteria used to determine whether
            // or not a collection of FIX message elements
            // is a match for this version entry:
            public List<VfxFixVersion_Rule> Rules = new List<VfxFixVersion_Rule>();
        }

        /// <summary>
        ///  The map of all versions that the matcher has been
        ///  initialized with, keyed by their respective layers.
        /// </summary>
        private Dictionary<string, List<MatchEntry>> _mapLayers = new Dictionary<string, List<MatchEntry>>();

        #region IVfxFixVxMatcher Members

        /// <summary>
        /// The Init method is invoked to initialize an instance
        /// of the matcher with a version definition registry that
        /// it can use to lookup information about vesions as it is
        /// matching messages.
        /// </summary>
        /// <param name="registry">
        /// The version definition registry that the matcher is to 
        /// use when looking up version information.
        /// </param>
        public void Init(IVfxFixVxRegistry registry)
        {
            // REC: Iterate over all of the version definitions
            // in the registry and construct corresponding match
            // entries for the matcher:
            foreach (VfxFixVxRecord vxRecord in registry)
            {
                // REC: The version name:
                string vxName = vxRecord.Name;

                MatchEntry matchEntry = new MatchEntry();

                matchEntry.Name = vxName;
                VfxFixVxRecord vxDetails = registry.Get(vxName);
                foreach (VfxFixVersion_Rule vxRule in vxDetails.Rules)
                {
                    matchEntry.Rules.Add(vxRule);
                }

                if (!_mapLayers.ContainsKey(vxDetails.Layer))
                {
                    _mapLayers.Add(vxDetails.Layer, new List<MatchEntry>());
                }

                _mapLayers[vxDetails.Layer].Add(matchEntry);
            }
        }

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
        public string GetSxVersion(FixCollection elements)
        {
            return GetVersion(elements, "session");
        }

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
        public string GetAxVersion(FixCollection elements)
        {
            string result = GetVersion(elements, "application");
            if (result == null)
            {
                result = GetVersion(elements, "combined");
                if (result == null)
                {
                    result = GetVersion(elements, "session");
                }
            }

            return result;
        }

        /// <summary>
        /// The GetVersion method attempts to determine the version
        /// of a specific layer of the FIX protocol, based on a set
        /// of FIX message elements.
        /// </summary>
        /// <param name="elements">
        /// The collection of FIX elements to analyze.
        /// </param>
        /// <param name="layer">
        /// The layer of the protocol version to retrieve.
        /// </param>
        /// <returns>
        /// The name of the protocol, at the specified layer, that
        /// matches the specified FIX message elements.
        /// </returns>
        public string GetVersion(FixCollection elements, string layer)
        {
            string result = null;

            if (_mapLayers.ContainsKey(layer))
            {
                foreach (MatchEntry matchEntry in _mapLayers[layer])
                {
                    bool matched = true;
                    foreach (VfxFixVersion_Rule rule in matchEntry.Rules)
                    {
                        foreach (VfxFixVersion_Field field in rule.Fields)
                        {
                            int nTag = int.Parse(field.Tag);
                            IFixElement element = elements.GetField(nTag);
                            if (element == null)
                            {
                                element = elements.GetGroup(nTag);
                            }

                            if (element != null)
                            {
                                string fieldContent = element.Content.ToString();
                                if (fieldContent.CompareTo(field.Value) != 0)
                                {
                                    matched = false;
                                }
                            }
                            else
                            {
                                matched = false;
                                break;
                            }
                        }

                        if (matched == true)
                        {
                            break;
                        }
                    }

                    if (matched == true)
                    {
                        result = matchEntry.Name;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
