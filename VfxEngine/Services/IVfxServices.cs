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

namespace VfxEngine.Services
{
    /// <summary>
    /// The IVfxServices interface is based on the .NET object
    /// model's ServiceContainer class and provides an interface
    /// for managing a generic collection of services. 
    /// </summary>
    public interface IVfxServices : IServiceProvider
    {
        /// <summary>
        /// The AddService method is invoked to add a new instance
        /// of a service implementation to the collection of services
        /// that is being managed. If there is already an instance of
        /// the specified service type in the collection, it will be 
        /// replaced with the specified instance.
        /// </summary>
        /// <param name="serviceType">
        /// The .NET type of the service being added.
        /// </param>
        /// <param name="serviceImpl">
        /// The implementation of the service being added.
        /// </param>
        void AddService(System.Type serviceType, object serviceImpl);

        /// <summary>
        /// The AddService method is invoked to add a new instance of
        /// a service implementation to the collection of services that
        /// is being managed. This version of the method adds the service
        /// under a specific name, rather than its .NET type.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the service being added.
        /// </param>
        /// <param name="serviceImpl">
        /// The implementation of the service being added.
        /// </param>
        void AddService(string serviceName, object serviceImpl);

        /// <summary>
        /// The GetService method is invoked to retrieve an instance
        /// of a service from the collection, using the .NET type of
        /// the service as the key for looking it up.
        /// </summary>
        /// <param name="serviceType">
        /// The .NET type of the service to retrieve.
        /// </param>
        /// <returns>
        /// The service implementation that is registered with the
        /// collection under the specified type, or null if there is
        /// no matching entry in the collection.
        /// </returns>
        //object GetService(System.Type serviceType);

        /// <summary>
        /// The GetService method is invoked to retrieve an instance
        /// of a service from the container, using the name that was
        /// used previously to add it to the container.
        /// </summary>
        /// <param name="name">
        /// The name of the service to retrieve.
        /// </param>
        /// <returns>
        /// The service implementation that is registered with the
        /// collection under the specified name, or null if there is
        /// no matching entry in the collection.
        /// </returns>
        object GetService(string serviceName);
    }
}
