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

using VfxEngine.Ipc;
using VfxEngine.Ipc.Endpoints;
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServiceFactory class creates instances of services
    /// for the engine. The service creation process is driven by the
    /// contents of the metabase settings for a specific session.
    /// </summary>
    public class VfxFixServiceFactory
    {
        /// <summary>
        /// The CreateService method is invoked to create a new instance
        /// of a service, based on the configuration information that is
        /// provided in the specified XML document.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static IVfxFixService CreateService(IVfxServices services, XmlDocument settings)
        {
            IVfxFixService result = null;

            XPathNavigator xpn = settings.CreateNavigator();
            XPathNodeIterator xpi = xpn.Select("/Session/Protocol/Settings/Setting[@name='Fix.Session.Role']");
            if((xpi.Count == 1) && (xpi.MoveNext()))
            {
                string role = xpi.Current.GetAttribute("content", "");
                if (!string.IsNullOrEmpty(role))
                {
                    switch (role)
                    {
                        case "Client":
                            result = new VfxFixClientService();
                            break;

                        case "Server":
                            result = new VfxFixServerService();
                            break;
                    }

                    if (result != null)
                    {
                        result.Init(services, settings);
                    }
                }
            }
            else
            {
                throw new ArgumentException("The setting 'Fix.Session.Role' was not found in the specified configuration.");
            }

            return result;
        }
    }
}
