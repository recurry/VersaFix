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

using VfxEngine.Fix;
using VfxEngine.FixDxRegistry;
using VfxEngine.FixVxRegistry;
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixEngine class is an implementation of a FIX engine
    /// using the Vortex FIX engine assemblies. The primary purpose
    /// of this class is to provide a black box implementation of a
    /// basic FIX engine that can be used as a foundation for more
    /// sophisticated applications.
    /// </summary>
    public class VfxFixEngine
    {
        /// <summary>
        /// The map of all services that are currently being managed
        /// by an instance of the engine, keyed by their names.
        /// </summary>
        private Dictionary<string, IVfxFixService> _mapServices = new Dictionary<string, IVfxFixService>();

        /// <summary>
        /// The Activate method starts all of the services that
        /// have been instantiated by the engine.
        /// </summary>
        public void Activate(IVfxServices services)
        {
            this._mapServices.Clear();

            // REC: Retrieve the engine configuration settings from
            // the service container, by retrieving the reference to
            // the IVfxSettings interface:
            IVfxSettings configuration = services.GetService(typeof(IVfxSettings)) as IVfxSettings;
            if (configuration != null)
            {
                // REC: Retrieve all of the session definitions from the
                // application configuration document:
                XPathNavigator xpn = configuration.Document.CreateNavigator();
                XPathNodeIterator xpi = xpn.Select("/Engine/Sessions/Session");
                while (xpi.MoveNext())
                {
                    string sessionName = xpi.Current.GetAttribute("name", "");
                    if (!string.IsNullOrEmpty(sessionName))
                    {
                        string sessionType = xpi.Current.GetAttribute("type", "");
                        if (sessionType != null)
                        {
                            // REC: Create a new XML document that represents the
                            // service's specific configuration details:
                            XmlDocument svxConfiguration = new XmlDocument();
                            svxConfiguration.LoadXml(xpi.Current.OuterXml);

                            IVfxFixService service = null;
                            switch (sessionType)
                            {
                                case "FIX":
                                    service = VfxFixServiceFactory.CreateService(services, svxConfiguration);
                                    break;
                                case "FAST":
                                    break;

                            }

                            this._mapServices.Add(sessionName, service);
                        }
                    }
                }
            }

            foreach (string key in _mapServices.Keys)
            {
                IVfxFixService service = _mapServices[key];
                service.Activate();
            }
        }

        /// <summary>
        /// The Shutdown method is invoked to shutdown all of
        /// the services that the engine is currently managing.
        /// </summary>
        public void Shutdown()
        {
            foreach (string key in _mapServices.Keys)
            {
                IVfxFixService service = _mapServices[key];
                service.Shutdown();
            }
        }
    }
}
