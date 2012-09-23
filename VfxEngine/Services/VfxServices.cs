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
using System.Threading;

namespace VfxEngine.Services
{
    /// <summary>
    /// The VfxAppServices class provides an implementation
    /// of the IVfxServices interface for managing a generic
    /// collection of service implementations.
    /// </summary>
    public class VfxServices : IVfxServices 
    {
        // REC: Synchronize read/write operations against
        // the map of services:
        private ReaderWriterLock _rwLock = new ReaderWriterLock();

        // REC: The map of all services that have been registered
        // with the service under a specific name:
        private Dictionary<string, object> _mapServicesByName = new Dictionary<string, object>();

        // REC: The map of all services registered with
        // the container, keyed by their .NET types:
        private Dictionary<System.Type, object> _mapServicesByType = new Dictionary<System.Type, object>();


        #region IVfxServices Members

        public void AddService(Type type, object instance)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            if (!_mapServicesByType.ContainsKey(type))
            {
                _mapServicesByType.Add(type, instance);
            }
            else
            {
                _mapServicesByType[type] = instance;
            }
            _rwLock.ReleaseWriterLock();
        }

        public void AddService(string serviceName, object serviceImpl)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            
            if (!_mapServicesByName.ContainsKey(serviceName))
            {
                _mapServicesByName.Add(serviceName, serviceImpl);
            }
            else
            {
                _mapServicesByName[serviceName] = serviceImpl;
            }

            _rwLock.ReleaseWriterLock();
        }


        public object GetService(Type type)
        {
            object result = null;
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            if (_mapServicesByType.ContainsKey(type))
            {
                result = _mapServicesByType[type];
            }
            _rwLock.ReleaseReaderLock();
            return result;
        }


        public object GetService(string serviceName)
        {
            object result = null;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            if (_mapServicesByName.ContainsKey(serviceName))
            {
                result = _mapServicesByName[serviceName];
            }
            _rwLock.ReleaseReaderLock();
            return result;
        }

        #endregion
    }
}
