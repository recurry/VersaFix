using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VfxEngine.Fix;
using VfxEngine.FixApp;

namespace Server
{
    class App : IVfxFixApp
    {
        public void OnSessionOpened(IVfxFixAppSession session)
        {
            Console.WriteLine("FIX Server: Session opened...");
        }

        public void OnSessionLogon(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            FixField fixTCID = msg.Header.GetField(56);

            Console.WriteLine(string.Format("FIX Server: {0} - LOGON", fixSCID.Content));


        }

        public void OnSessionLogout(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            FixField fixTCID = msg.Header.GetField(56);

            Console.WriteLine(string.Format("FIX SERVER: {0} - LOGOUT.", fixSCID.Content));

        }

        public void OnSessionRxAdmMessage(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            Console.WriteLine(string.Format("FIX SERVER: {0} SENT: {1}", fixSCID.Content, msg.ToString()));

        }

        public void OnSessionRxAppMessage(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            Console.WriteLine(string.Format("FIX SERVER: {0} SENT: {1}", fixSCID.Content, msg.ToString()));
        }

        public void OnSessionTxAdmMessage(IVfxFixAppSession session, FixMessage msg)
        {

        }

        public void OnSessionTxAppMessage(IVfxFixAppSession session, FixMessage msg)
        {

        }

        public void OnSessionClosed(IVfxFixAppSession session)
        {

        }
    }
}
