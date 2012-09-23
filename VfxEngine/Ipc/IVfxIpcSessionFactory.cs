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

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The IVfxIpcSessionFactory defines an interface that can
    /// be implemented to support session creation when working
    /// with the VfxTcpAcceptor, or another class that needs to
    /// be able to create instances of IPC sessions without any
    /// knowledge of the actual session type.
    /// </summary>
    public interface IVfxIpcSessionFactory
    {
        /// <summary>
        /// The CreateSession interface simply creates a new 
        /// instance of a specific type of IPC session and then
        /// returns that instance to the caller.
        /// </summary>
        /// <returns></returns>
        IVfxIpcSession CreateSession();
    }
}
