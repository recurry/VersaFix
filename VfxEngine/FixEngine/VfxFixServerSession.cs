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

using VfxEngine.FixApp;
using VfxEngine.FixSessions;
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServerSession class provides an implementation
    /// of the IVfxFixAppSession interface that can be passed to
    /// an instance of IVfxFixApp as the session parameter for a
    /// callback method. 
    /// </summary>
    public sealed class VfxFixServerSession : IVfxFixAppSession
    {
        /// <summary>
        /// The _fixSession member is used to maintain a reference
        /// to the actual FIX session that the server has established
        /// with a peer system.
        /// </summary>
        private IVfxFixSession _fixSession;

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the server session encapsulates
        /// in order to forward requests that are made through the
        /// implementations of the IVfxFixAppSession methods.
        /// </param>
        public VfxFixServerSession(IVfxFixSession session)
        {
            _fixSession = session;
        }

        #region IVfxFixAppSession Members

        public Guid InstanceId
        {
            get { return _fixSession.InstanceId; }
        }

        /// <summary>
        /// This implementation of the Send method forwards the
        /// specified FIX message to the underlying FIX session
        /// for transmission to the peer system.
        /// </summary>
        /// <param name="msg"></param>
        public void Send(VfxEngine.Fix.FixMessage msg)
        {
            _fixSession.HandleTxMessage(msg);
        }

        #endregion
    }
}
