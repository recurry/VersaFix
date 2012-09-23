using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VfxEngine.Fix;
using VfxEngine.FixApp;

namespace Client
{
    public class App : IVfxFixApp
    {
        public void OnSessionOpened(IVfxFixAppSession session)
        {
            Console.WriteLine("FIX Client: Session opened...");
        }

        public void OnSessionLogon(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            FixField fixTCID = msg.Header.GetField(56);

            Console.WriteLine(string.Format("FIX CLIENT: {0} - LOGON", fixSCID.Content));
            System.Threading.ThreadPool.QueueUserWorkItem(HandleWork_SendMessages, session);
        }

        private void HandleWork_SendMessages(object state)
        {
            int orderId = 1;

            IVfxFixAppSession session = state as IVfxFixAppSession;

            for (int i = 0; i != 5; i++)
            {
                FixMessage msg = new FixMessage();

                msg.Header.SetField(new FixField(35, "8"));

                msg.Content.SetField(new FixField(37, orderId++.ToString()));
                msg.Content.SetField(new FixField(17, "5000"));
                msg.Content.SetField(new FixField(20, "0"));
                msg.Content.SetField(new FixField(150, "0"));
                msg.Content.SetField(new FixField(39, "0"));
                msg.Content.SetField(new FixField(55, "AAPL"));
                msg.Content.SetField(new FixField(54, "1"));
                msg.Content.SetField(new FixField(151, "3000"));
                msg.Content.SetField(new FixField(14, "0"));
                msg.Content.SetField(new FixField(6, "0"));

                FixGroup grpTest = new FixGroup(382, "2");
                FixCollection grpInstance = new FixCollection();
                grpInstance.AddField(new FixField(375, "foo"));
                grpInstance.AddField(new FixField(337, "bar"));
                grpTest.Instances.Add(grpInstance);
                grpTest.Instances.Add(grpInstance);

                msg.Content.AddGroup(grpTest);

                session.Send(msg);
            }

            if (session != null)
            {
                for (int i = 0; i != 10000; i++)
                {
                    FixMessage message = new FixMessage();

                    message.Header.AddField(new FixField(35, "D"));
                    session.Send(message);
                }
            }
        }


        public void OnSessionLogout(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            FixField fixTCID = msg.Header.GetField(56);

            Console.WriteLine(string.Format("FIX CLIENT: {0} - LOGOUT.", fixSCID.Content));

        }

        public void OnSessionRxAdmMessage(IVfxFixAppSession session, FixMessage msg)
        {
            FixField fixSCID = msg.Header.GetField(49);
            Console.WriteLine(string.Format("FIX CLIENT: {0} SENT: {1}", fixSCID.Content, msg.ToString()));

        }

        public void OnSessionRxAppMessage(IVfxFixAppSession session, FixMessage msg)
        {

        }

        public void OnSessionTxAdmMessage(IVfxFixAppSession session, FixMessage msg)
        {

        }

        public void OnSessionTxAppMessage(IVfxFixAppSession session, FixMessage msg)
        {

        }

        public void OnSessionClosed(IVfxFixAppSession session)
        {
            Console.WriteLine("FIX CLIENT: - Session Closed.");
        }
    }
}
