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

namespace VfxEngine.Ipc.Endpoints
{
    /// <summary>
    /// The VfxEndpointFactory creates new instances of endpoints
    /// based on an endpoint configuration found in a section of an
    /// application's configuration metabase.
    /// </summary>
    public sealed class VfxEndpointFactory
    {
        /// <summary>
        /// The Create method creates a new instance of an endpoint
        /// implementation, based on the settings that are found in
        /// a section of a configuration metabase.
        /// </summary>
        /// <param name="settings">
        /// The section of a configuration metabase that contains 
        /// configuration details for a specific endpoint type.
        /// </param>
        /// <returns>
        /// The resulting IPC endpoint.
        /// </returns>
        public static IVfxEndpoint Create(XmlDocument settings)
        {
            IVfxEndpoint result = null;

            // REC: Retrieve the endpoint type from the provided
            // settings and determine whether or not an instance
            // of that type can be created:
            XPathNavigator xpn = settings.CreateNavigator();
            XPathNodeIterator xpi = xpn.Select("/Endpoint");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                string requestedType = xpi.Current.GetAttribute("type", "");
                if (!string.IsNullOrEmpty(requestedType))
                {
                    switch (requestedType)
                    {
                        case "Tcp.Acceptor":
                            result = new VfxTcpServerEndpoint();
                            break;
                        case "Tcp.Connector":
                            result = new VfxTcpClientEndpoint();
                            break;
                    }
                }
                else
                {
                    throw new ArgumentException("An endpoint type must be specified.");
                }

            }

            if(result == null)
            {
                throw new ArgumentException("The specified endpoint type is not supported.");
            }

            result.Initialize(settings);
            return result;
        }
    }
}
