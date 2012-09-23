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
    /// The IVfxIpcSession interface defines the interface that
    /// must be implemented by sessions that are created through
    /// an instance of a session factory. Typically, an acceptor
    /// creates an instance of a session and initializes it with
    /// an implementation-specific IPC module.
    /// </summary>
    public interface IVfxIpcSession
    {
        /// <summary>
        /// The Init interface is called by an acceptor or any
        /// other mechanism that initializes sessions to provide
        /// the session with the IPC module that it should use
        /// for communicating with a remote peer. 
        /// </summary>
        /// <param name="ipcModule"></param>
        void Init(IVfxIpcModule ipcModule);

    }
}
