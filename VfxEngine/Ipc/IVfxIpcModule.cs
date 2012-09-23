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
    /// The IVfxIpcModule interface defines the basic interface that
    /// must be implemented by asynchronous IO processing modules in
    /// the VFX engine's IPC system.
    /// </summary>
    public interface IVfxIpcModule
    {
        /// <summary>
        /// The EventDispatch event is a multiplexing event that is
        /// used by the module to report various events to its owner.
        /// </summary>
        event EventHandler<VfxIpcEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method is invoked to instruct the IPC module
        /// to start asynchronously processing IO.
        /// </summary>
        void Activate();

        /// <summary>
        /// The Shutdown method is invoked to instruct the IPC module
        /// to terminate the IPC connection. The graceful flag indicates
        /// whether or not the module should shutdown gracefully or just
        /// terminate the connection immediately.
        /// </summary>
        void Shutdown(bool graceful);

        /// <summary>
        /// The HandleTx method is invoked to instruct the IPC module
        /// to transmit a message to the peer system.
        /// </summary>
        /// <param name="mb">
        /// The IPC message block that contains the data that is to be
        /// transmitted to the peer system.
        /// </param>
        void HandleTx(VfxMsgBlock mb);

    }
}
