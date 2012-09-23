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
    /// The IVfxService interface defines the interface that
    /// must be implemented by VersaFix services.
    /// </summary>
    public interface IVfxFixService
    {
        /// <summary>
        /// The dispatch event that allows subscribers to receive
        /// events that are dispatched by the service.
        /// </summary>
        event EventHandler<VfxFixServiceEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method starts the service. If the service
        /// activation is asynchronous, an event will be dispatched
        /// to the service's owner when the activation completes.
        /// </summary>
        void Activate();

        /// <summary>
        /// The Init method is called to initialize an instance
        /// of a service with its service specific configuration.
        /// </summary>
        /// <param name="services">
        /// A reference to a service container that provides the
        /// service with any external services it may required.
        /// </param>
        /// <param name="settings">
        /// The service's configuration details as they are defined
        /// in the VersaFix configuration metabase.
        /// </param>
        void Init(IVfxServices services, XmlDocument settings);

        /// <summary>
        /// The Shutdown method stops a service. If the service
        /// shutdown is asynchronous, an event will be dispatched
        /// to the service's owner when the shutdown completes.
        /// </summary>
        void Shutdown();

    }
}
