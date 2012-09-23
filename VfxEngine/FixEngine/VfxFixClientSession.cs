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
    /// The VfxFixClientSession class provides an implementation
    /// of the IVfxFixAppSession interface that can be passed to
    /// an instance of IVfxFixApp as the session parameter for a
    /// callback method. 
    /// </summary>
    public sealed class VfxFixClientSession : IVfxFixAppSession
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
        /// The IVfxFixSession implementation that the client
        /// session forwards the appropriate requests to.
        /// </param>
        public VfxFixClientSession(IVfxFixSession session)
        {
            _fixSession = session;
        }

        #region IVfxFixAppSession Members

        /// <summary>
        /// The unique identifier that has been assigned to an
        /// instance of a client session, for use in identifying
        /// the session within an application.
        /// </summary>
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
