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
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixDatabase
{
    /// <summary>
    /// The IVfxFixDatabase interface defines an interface
    /// for FIX session database implementations.
    /// </summary>
    public interface IVfxFixDatabase
    {
        /// <summary>
        /// The Init method is invoked by the database factory to
        /// initialize a new instance of a database implementation.
        /// </summary>
        /// <param name="services">
        /// The services that are provided to the database by the
        /// owner of the database factory that creates it.
        /// </param>
        /// <param name="settings">
        /// The configuration settings for the database instance 
        /// as defined in the application's configuration metabase.
        /// </param>
        void Init(IVfxServices services, XmlDocument settings);

        /// <summary>
        /// The AcquireSession method is invoked to request 
        /// exclusive access to a persisted session in the
        /// database. The session's persisted information is
        /// returned as an instance of VfxFixDatabaseEntry.
        /// An implementation of this interface should take
        /// into consideration a timeout or other setting that
        /// will ensure the session does not remain locked for
        /// an indefinite period of time. 
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the persisted FIX session that 
        /// is to be acquired.
        /// </param>
        /// <returns>
        /// An instance of VfxFixDatabaseEntry that contains 
        /// the details of the persisted session.
        /// </returns>
        VfxFixDatabaseRecord AcquireSession(string sessionId);

        /// <summary>
        /// The ResetSession method completely resets an instance
        /// of a session in the database. This clears the session
        /// entry and the message store for the session.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the session to be reset.
        /// </param>
        void ResetSession(string sessionId);

        /// <summary>
        /// The AddMessage method adds an instance of a message
        /// to the database.
        /// </summary>
        /// <param name="sessionId">
        /// The session that the message relates to.
        /// </param>
        /// <param name="msg">
        /// The FIX message to be added to the database.
        /// </param>
        void AddMessage(string sessionId, FixMessage msg);

        
        /// <summary>
        /// The ReleaseSession method is invoked to request the
        /// database release a session's lock on an instance of
        /// a persisted session.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the persisted session that is being
        /// released by the caller.
        /// </param>
        void ReleaseSession(string sessionId);

        /// <summary>
        /// The ReleaseSessiom method is invoked to request the
        /// database release a session's lock on an instance of
        /// a persisted session and to simultaneously update the
        /// session record associated with the session.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the persisted session that is being
        /// released by the caller.
        /// </param>
        /// <param name="record">
        /// The updates session record for the session that is to
        /// be written back to the database.
        /// </param>
        void ReleaseSession(string sessionId, VfxFixDatabaseRecord record);
    }
}
