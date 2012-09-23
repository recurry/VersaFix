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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;

using VfxEngine.Fix;
using VfxEngine.Settings;
using VfxEngine.Sequencer;
using VfxEngine.Services;

namespace VfxEngine.FixDatabase
{
    /// <summary>
    /// The VfxFixFileDatabase class provides an implementation
    /// of a FIX session database that uses files as the backing
    /// store for all of the information in the database.
    /// </summary>
    public sealed class VfxFixFileDatabase : IVfxFixDatabase
    {
        /// <summary>
        /// The path that the database uses as the root for
        /// storing all of its files and related information.
        /// </summary>
        private string _rootPath;

        /// <summary>
        /// The IndexEntry class is an internal class that
        /// is used to maintain information about a single
        /// entry in the message index file.
        /// </summary>
        private class IndexEntry
        {
            /// <summary>
            /// The sequence number of the FIX message that
            /// the index is related to.
            /// </summary>
            public int msgSequence;

            /// <summary>
            /// The offset of the FIX message in the file
            /// store that contains the session's messages.
            /// </summary>
            public long msgOffset;

            /// <summary>
            /// The length of the FIX message in the file
            /// store that contains the session's messages.
            /// </summary>
            public long msgLength;
        }

        /// <summary>
        /// The SessionDetails class is an internal class that
        /// is used to keep track of the file streams and other
        /// information related to an active session.
        /// </summary>
        private class SessionDetails
        {
            /// <summary>
            /// The stream that is opened for reading and writing
            /// the session's index file.
            /// </summary>
            public StreamWriter IdxWriter;

            /// <summary>
            /// The stream that is opened for reading and writing
            /// the session's message store.
            /// </summary>
            public StreamWriter MsgWriter;

            /// <summary>
            /// The list of all index entries that are in the
            /// message index for the session:
            /// </summary>
            public List<IndexEntry> Index = new List<IndexEntry>();
        }

        /// <summary>
        /// The TaskDetails_Add class encapsulates the details for
        /// an asynchronous add message request that is to be added
        /// to the sequencer for execution on the .NET thread pool.
        /// </summary>
        private class TaskDetails_Add
        {
            private string _sessionId;

            /// <summary>
            /// The session identifier that is associated with
            /// the asynchronous add request.
            /// </summary>
            public string SessionId
            {
                get { return _sessionId; }
            }

            private FixMessage _sessionMsg;

            /// <summary>
            /// The FIX message that is to be added to the message
            /// store associated with the session.
            /// </summary>
            public FixMessage SessionMsg
            {
                get { return _sessionMsg; }
            }

            public TaskDetails_Add(string sessionId, FixMessage sessionMsg)
            {
                _sessionId = sessionId;
                _sessionMsg = sessionMsg;
            }
        }

        /// <summary>
        /// The map of all open sessions that the database is
        /// currently managing, keyed by their session ids.
        /// </summary>
        private Dictionary<string, SessionDetails> _mapSessions = new Dictionary<string, SessionDetails>();

        /// <summary>
        ///  The services that are provided to the database
        ///  implementation when it is initialized.
        /// </summary>
        private IVfxServices _services;

        /// <summary>
        /// The task sequencer that the file database uses
        /// to enqueue asynchronous operations.
        /// </summary>
        private VfxSequencer _sequencer = new VfxSequencer();

        /// <summary>
        /// The Init method is invoked to initialize an instance
        /// of VfxFileDatabase with a specific configuration. This
        /// configuration is usually retrieved from a configuration
        /// metabase file associated with an application.
        /// </summary>
        /// <param name="services">
        /// The reference to a service container that has any service
        /// implementations that the file database requires.
        /// </param>
        /// <param name="settings">
        /// The configuration settings for the file database that 
        /// are specified in the configuration metabase.
        /// </param>
        public void Init(IVfxServices services, XmlDocument settings)
        {
            // REC: Retrieve the path that the database uses
            // as the root of its file storage area:
            XPathNavigator xpn = settings.CreateNavigator();
            XPathNodeIterator xpi = xpn.Select("/Database/Settings/Setting[@name='Database.Path']");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                string path = xpi.Current.GetAttribute("content", "");
                this._rootPath = path;
            }

            // REC: Retain a reference to the service container
            // in order to access the services that may be needed
            // by the database instance:
            this._services = services;
        }

        #region IVfxFixDatabase Members

        /// <summary>
        /// The AcquireSession method is invoked to request exclusive
        /// access to a session's information in the database.
        /// </summary>
        /// <param name="sessionId">
        /// The session's unique identifier.
        /// </param>
        /// <returns>
        /// The persisted information that is maintained for
        /// the specified session identifier.
        /// </returns>
        public VfxFixDatabaseRecord AcquireSession(string sessionId)
        {
            VfxFixDatabaseRecord result = new VfxFixDatabaseRecord();

            // REC: Determine if the session's file path exists:
            string sessionPath = Path.Combine(_rootPath, sessionId);
            if (!Directory.Exists(sessionPath))
            {
                Directory.CreateDirectory(sessionPath);
            }

            // REC: Attempt to open and load the database entry
            // for the specified session:
            string sessionFile = Path.Combine(sessionPath, "Session.xml");
            if (!File.Exists(sessionFile))
            {
                ResetSession(sessionId);
            }

            // REC: Attempt to read the session's details from
            // the persisted information in the session file:
            FileStream fsSession = new FileStream(sessionFile, FileMode.Open, FileAccess.Read, FileShare.None);
            XmlSerializer xsSession = new XmlSerializer(typeof(XmlFixDatabaseRecord));
            XmlFixDatabaseRecord sessionRecord = xsSession.Deserialize(fsSession) as XmlFixDatabaseRecord;
            result.TxSequence = sessionRecord.TxSequence;
            result.RxSequence = sessionRecord.RxSequence;
            fsSession.Close();

            // REC: Create the session's lock file:
            string lockFile = Path.Combine(sessionPath, "Locked.txt");
            FileStream fs = new FileStream(lockFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            fs.Close();

            // REC: Create a new instance of the SessionDetails class
            // that will be used to maintain the session's information:
            SessionDetails sessionDetails = new SessionDetails();

            // REC: Attempt to open the index file for the session:
            string idxFile = Path.Combine(sessionPath, "Index.xml");
            FileStream idxStream = new FileStream(idxFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            // REC: Maintain a reference to the index file stream
            // in the session's details record:
            sessionDetails.IdxWriter = new StreamWriter(idxStream);

            // REC: Read all of the index entries from the index file
            // and store them in the session's details structure:
            StreamReader idxReader = new StreamReader(idxStream);
            while (idxReader.EndOfStream == false)
            {
                string idxLine = idxReader.ReadLine();
                string[] idxTokens = idxLine.Split(new char[] { ':' });

                IndexEntry idxEntry = new IndexEntry();
                idxEntry.msgOffset = int.Parse(idxTokens[1]);
                idxEntry.msgLength = int.Parse(idxTokens[2]);
                idxEntry.msgSequence = int.Parse(idxTokens[0]);

                sessionDetails.Index.Add(idxEntry);
            }

            // REC: Attempt to open the message file for the session:
            string msgFile = Path.Combine(sessionPath, "Messages.txt");
            FileStream msgStream = new FileStream(msgFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            // REC: Maintain a reference to the message file stream
            // in the session's details record:
            sessionDetails.MsgWriter = new StreamWriter(msgStream);

            // REC: Add the session details record to the internal
            // map, keyed by the session identifier:
            _mapSessions.Add(sessionId, sessionDetails);

            // REC: Return the session's specifics to the caller:
            return result;
        }

        /// <summary>
        /// The AddMessage method adds an instance of a FIX message
        /// to the database file that is associated with the specified
        /// session. The index file is updated to reflect the position
        /// in the database of the message that is being added.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the session the message relates to.
        /// </param>
        /// <param name="msg">
        /// The FIX message to be added to the database.
        /// </param>
        public void AddMessage(string sessionId, FixMessage msg)
        {
            // REC: Create the details instance for the task:
            TaskDetails_Add taskDetails = new TaskDetails_Add(sessionId, msg);
            // REC: Enqueue the task for asynchronous execution:
            _sequencer.Enqueue(AddMessage_Entrypoint, taskDetails);
        }

        /// <summary>
        /// The ResetSession method completely resets an instance
        /// of a session that is stored in the database.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the session to be reset.
        /// </param>
        public void ResetSession(string sessionId)
        {
            // REC: Ensure that the session is not locked:
            string sessionPath = Path.Combine(_rootPath, sessionId);
            string sessionLock = Path.Combine(sessionPath, "Locked.txt");
            if (File.Exists(sessionLock))
            {
                string error = string.Format("Session \"{0}\" is locked.", sessionId);
                throw new InvalidOperationException(error);
            }

            // REC: Remove the session's persisted session info:
            string sessionFile = Path.Combine(sessionPath, "Session.xml");
            if (File.Exists(sessionFile))
            {
                File.Delete(sessionFile);
            }

            // REC: Remove the session's index file:
            string sessionIdxFile = Path.Combine(sessionPath, "index.xml");
            if (File.Exists(sessionIdxFile))
            {
                File.Delete(sessionIdxFile);
            }

            string sessionMsgFile = Path.Combine(sessionPath, "messages.dat");
            if (File.Exists(sessionMsgFile))
            {
                File.Delete(sessionMsgFile);
            }

            // REC: Create a new session record for the session:
            XmlFixDatabaseRecord xmlEntry = new XmlFixDatabaseRecord();
            xmlEntry.TxSequence = 1;
            xmlEntry.RxSequence = 1;
            
            string[] compIDs = sessionId.Split(new char[] { '-' });
            xmlEntry.SenderCompID = compIDs[0];
            xmlEntry.TargetCompID = compIDs[1];

            // REC: Write the record out to the session file:
            FileStream fs = new FileStream(sessionFile, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlSerializer xs = new XmlSerializer(typeof(XmlFixDatabaseRecord));
            xs.Serialize(fs, xmlEntry);
            fs.Close();
        }


        /// <summary>
        /// This implementation of the ReleaseSession method is
        /// invoked to release any locks that are currently being
        /// held on the specified session.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the session being released.
        /// </param>
        public void ReleaseSession(string sessionId)
        {
            // REC: Ensure that the session id corresponds to 
            // an actual instance of an acquired session:
            if (_mapSessions.ContainsKey(sessionId))
            {
                // REC: Retrieve the session details for the
                // specifies session:
                SessionDetails details = _mapSessions[sessionId];

                // REC: Close the writers that are associated
                // with the specified session:
                details.IdxWriter.Close();
                details.MsgWriter.Close();

                // REC: Test for the presence of a lock file for
                // the specified session and delete it if found:
                string lockPath = Path.Combine(_rootPath, sessionId);
                string lockFile = Path.Combine(lockPath, "Locked.txt");
                if (File.Exists(lockFile))
                {
                    File.Delete(lockFile);
                }

                // REC: Remove the session details from the map:
                _mapSessions.Remove(sessionId);
            }
        }

        /// <summary>
        /// This implementation of the ReleaseSession method is 
        /// invoked to simultaneously release any locks that are
        /// currently being held on a session and update the info
        /// for the session in the database.
        /// </summary>
        /// <param name="sessionId">
        /// The session id of the session being released.
        /// </param>
        /// <param name="record">
        /// The updated record for the session being released.
        /// </param>
        public void ReleaseSession(string sessionId, VfxFixDatabaseRecord record)
        {
            string sessionPath = Path.Combine(_rootPath, sessionId);
            string sessionFile = Path.Combine(sessionPath, "Session.xml");

            // REC: Write the updated session details out to
            // the session record file:
            XmlFixDatabaseRecord xmlRecord = new XmlFixDatabaseRecord();
            xmlRecord.TxSequence = record.TxSequence;
            xmlRecord.RxSequence = record.RxSequence;

            FileStream fsSession = new FileStream(sessionFile, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlSerializer xsSession = new XmlSerializer(typeof(XmlFixDatabaseRecord));
            xsSession.Serialize(fsSession, xmlRecord);
            fsSession.Close();

            // REC: Close the index and message database files since
            // they are no longer needed:
            SessionDetails sessionDetails = _mapSessions[sessionId];
            sessionDetails.IdxWriter.Close();
            sessionDetails.MsgWriter.Close();

            // REC: Delete the session's lock file:
            string lockPath = Path.Combine(_rootPath, sessionId);
            string lockFile = Path.Combine(lockPath, "Locked.txt");
            if (File.Exists(lockFile))
            {
                File.Delete(lockFile);
            }

            // REC: Remove the session details from the map:
            _mapSessions.Remove(sessionId);
        }

        #endregion

        /// <summary>
        /// The AddMessage_Entrypoint method is the asynchronous
        /// entrypoint for processing add message requests that have
        /// been enqueued to the .NET thread pool through the instance
        /// of the sequencer that is associated with the database.
        /// </summary>
        /// <param name="state">
        /// The instance of TaskDetails_Add that is associated with
        /// the asynchronous add message request being processed.
        /// </param>
        private void AddMessage_Entrypoint(object state)
        {
            TaskDetails_Add taskDetails = state as TaskDetails_Add;
            if (taskDetails != null)
            {
                FixField fldSequence = taskDetails.SessionMsg.Header.GetField(34);
                if (fldSequence != null)
                {
                    // REC: Retrieve the message's text so that its
                    // length can be assigned to its index entry:
                    string msgText = taskDetails.SessionMsg.ToString();

                    // REC: Create an entry for the message in the
                    // index file associated with the session:
                    if (_mapSessions.ContainsKey(taskDetails.SessionId))
                    {
                        SessionDetails details = _mapSessions[taskDetails.SessionId];

                        IndexEntry idxEntry = new IndexEntry();
                        idxEntry.msgOffset = details.MsgWriter.BaseStream.Position;
                        idxEntry.msgLength = msgText.Length;
                        idxEntry.msgSequence = int.Parse(fldSequence.Content);

                        details.Index.Add(idxEntry);

                        details.MsgWriter.Write(msgText.ToCharArray());
                        details.MsgWriter.Flush();

                        string idxLine = string.Format("{0}:{1}:{2}", idxEntry.msgSequence, idxEntry.msgOffset, idxEntry.msgLength);
                        details.IdxWriter.WriteLine(idxLine.ToCharArray());
                        details.IdxWriter.Flush();
                    }
                }
            }
        }
    }
}
