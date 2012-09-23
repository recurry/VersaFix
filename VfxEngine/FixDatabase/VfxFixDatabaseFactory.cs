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

using System.Xml;
using System.Xml.XPath;

using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixDatabase
{
    /// <summary>
    /// The VfxFixDatabaseFactory creates instances of session
    /// database implementations (IVfxFixDatabase) based on the
    /// configuration settings that are supplied to it.
    /// </summary>
    public sealed class VfxFixDatabaseFactory
    {
        /// <summary>
        /// The Create method is invoked to create a new instance of
        /// a specific type of database based on the configuration settings
        /// that are provided in the IVfxSettings implementation that is 
        /// passed to the method.
        /// </summary>
        /// <param name="services">
        /// The services container that contains references to any service
        /// implementations the factory requires access to.
        /// </param>
        /// <param name="settings">
        /// The collection of configuration settings to be examined by the
        /// factory in determining what type of database implementation is
        /// to be created and returned to the caller.
        /// </param>
        /// <returns>
        /// The resulting database implementation, or null if the factory
        /// could not create one based on the provided settings.
        /// </returns>
        public static IVfxFixDatabase Create(IVfxServices services, XmlDocument settings)
        {
            IVfxFixDatabase result = null;

            XPathNavigator xpn = settings.CreateNavigator();
            XPathNodeIterator xpi = xpn.Select("/Database");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                string requestedType = xpi.Current.GetAttribute("type", "");
                switch (requestedType)
                {
                    case "file":
                        result = new VfxFixFileDatabase();
                        break;
                }

                if (result != null)
                {
                    result.Init(services, settings);
                }
            }

            return result;
        }
    }
}
