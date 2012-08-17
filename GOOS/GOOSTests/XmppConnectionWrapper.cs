using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using GOOS;
using NUnit.Framework;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.vcard;

namespace GOOSTests
{
    /* You will need this user config in vysper-0.7\config\spring-config.xml for these tests
     * 
     * <bean id="addUsers" class="org.apache.vysper.spring.AddUserHelper">
        <constructor-arg index="0">
            <map>
    			<entry key="sniper@vysper.org" value="sniper" />
            </map>
        </constructor-arg>
        <property name="storageProviderRegistry" ref="storageRegistry" />
    </bean>
     * */
    [TestFixture]
    public class XmppConnectionWrapperTests
    {
        const string ValidUserName = "sniper";
        const string ValidPassword = "sniper";
        const string Server = "localhost";
        
        private static void WaitForAuthentication(ITestableConnection conn)
        {
            bool waitForAuthentication = conn.WaitForAuthentication(5000);
            Assert.That(waitForAuthentication, Is.True, "Did not authenticate within given time span");
        }

        private static void WaitForMessageReceived(ITestableConnection conn)
        {
            bool wait = conn.WaitForMessageRecieved(5000);
            Assert.That(wait, Is.True, "Did not receive message within given time span");
        }

        [Test]
        public void Wrapper_ShouldHaveStatusOfDisconnected_WhenFirstCreated()
        {
            var conn = new XmppConnectionWrapper(ValidUserName, ValidPassword, Server);
            Assert.That(conn.Status, Is.EqualTo(ConnectionStatus.New));
        }

        [Test]
        public void Status_ShouldBeConnecting_ImmediatelyAfterOpenIsCalled()
        {
            var conn = new XmppConnectionWrapper(ValidUserName, ValidPassword, Server);
            conn.Open();
            Assert.That(conn.Status, Is.EqualTo(ConnectionStatus.Connecting));
        }

        [Test]
        public void Status_ShouldBeAuthenticated_IfWeUseValidUserNameAndPassword()
        {
            ITestableConnection conn = new XmppConnectionWrapper(ValidUserName, ValidPassword, Server);
            conn.Open();
            WaitForAuthentication(conn);

            Assert.That(conn.Status, Is.EqualTo(ConnectionStatus.Authenticated));
        }

        [Test]
        public void Status_ShouldBeAuthenticatedFailed_IfWeUseInValidUserNameAndPassword()
        {
            ITestableConnection conn = new XmppConnectionWrapper(ValidUserName, "wrong password", Server);
            conn.Open();
            WaitForAuthentication(conn);
            Assert.That(conn.Status, Is.EqualTo(ConnectionStatus.AuthenticationFailed));
        }

        [Test]
        public void Status_ShouldBeClosing_ImmediatelyAfterWeCallClose()
        {
            ITestableConnection conn = new XmppConnectionWrapper(ValidUserName, "wrong password", Server);
            conn.Open();
            WaitForAuthentication(conn);
            conn.Close();
            Assert.That(conn.Status, Is.EqualTo(ConnectionStatus.Closing));
        }

        [Test]
        public void TestSendingMessage_ToYourSelf_ShouldCauseMessageToBeReceieved()
        {
            ITestableConnection conn = new XmppConnectionWrapper(ValidUserName, ValidPassword, Server);
            conn.Open();
            WaitForAuthentication(conn);

            conn.SendMessage(new Jid(ValidUserName, Server, null), "donkey", "hello");
            WaitForMessageReceived(conn);

            Message lastMessage = conn.LastMessage;
            Assert.That(lastMessage.Body, Is.EqualTo("hello"));
            Assert.That(lastMessage.Subject, Is.EqualTo("donkey"));
        }

        [Test]
        public void TestSendingMessage_ToYourSelf_ShouldCauseMessageReceievedEventToBeRaised()
        {
            ITestableConnection conn = new XmppConnectionWrapper(ValidUserName, ValidPassword, Server);
            conn.Open();
            WaitForAuthentication(conn);

            Message lastMessage = null;
            conn.MessageReceived += delegate(object sender, MessageEventArgs args)
                                        {
                                            lastMessage = args.Message;
                                        };

            conn.SendMessage(new Jid(ValidUserName, Server, null), "donkey", "hello");
            WaitForMessageReceived(conn);

            Assert.That(lastMessage.Body, Is.EqualTo("hello"));
            Assert.That(lastMessage.Subject, Is.EqualTo("donkey"));
        }

    }

    public enum ConnectionStatus
    {
        New,
        Connecting,
        Authenticated,
        AuthenticationFailed,
        Closing,
        Closed
    }

    public class MessageEventArgs : EventArgs
    {
        public Message Message{get;private set;}

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
    public interface IConnection
    {
        ConnectionStatus Status{get;}
        void Open();
        void SendMessage(Jid to, string subject, string message);
        void Close();
        event EventHandler<MessageEventArgs> MessageReceived;
    }

    internal interface ITestableConnection : IConnection
    {
        /// <summary>
        /// Waits for the connection to either succeed or fail to authenticate
        /// <see cref="ConnectionStatus.Authenticated"/>,
        /// </summary>
        /// <param name="timeToWaitInMilliseconds"></param>
        bool WaitForAuthentication(int timeToWaitInMilliseconds);
        /// <summary>
        /// Waits for a message to be received.
        /// </summary>
        /// <param name="timeToWaitInMilliseconds"></param>
        /// <returns></returns>
        bool WaitForMessageRecieved(int timeToWaitInMilliseconds);

        void ResetMessageReceivedSemaphor();

        Message LastMessage{get;}
    }

    public class XmppConnectionWrapper : ITestableConnection
    {
        private readonly XmppClientConnection xmppCon;
        private readonly Jid jid;
        private Vcard myVcard;

        private readonly List<RosterItem> roster = new List<RosterItem>();
        private readonly ObservableCollection<Gent> onlineGents = new ObservableCollection<Gent>();

        private ConnectionStatus status;
        public ConnectionStatus Status{get {return status;}}

        public Element LastErrorMessage{get;private set;}
        private readonly ManualResetEvent connectionEstablishedEvent;
        private readonly ManualResetEvent messageReceivedEvent;

        public event EventHandler<MessageEventArgs> MessageReceived;

        public XmppConnectionWrapper(string userName, string password, string server)
        {
            xmppCon = new XmppClientConnection();
            jid = new Jid(userName + "@" + server);

            xmppCon.Password = password;
            xmppCon.Username = jid.User;
            xmppCon.Server = server;

            xmppCon.AutoAgents = false;
            xmppCon.AutoPresence = true;
            xmppCon.AutoRoster = true;
            xmppCon.AutoResolveConnectServer = true;

            xmppCon.OnRosterStart += xmppCon_OnRosterStart;
            xmppCon.OnRosterEnd += xmppCon_OnRosterEnd;
            xmppCon.OnRosterItem += xmppCon_OnRosterItem;
            xmppCon.OnPresence += xmppCon_OnPresence;
            xmppCon.OnMessage += xmppCon_OnMessage;
            xmppCon.OnLogin += xmppCon_OnLogin;
            xmppCon.OnAuthError += xmppCon_OnAuthError;
            xmppCon.OnIq += xmppCon_OnIq;

            xmppCon.OnError += xmppCon_OnError;
            xmppCon.OnClose += xmppCon_OnClose;

            status = ConnectionStatus.New;
            connectionEstablishedEvent = new ManualResetEvent(false);
            messageReceivedEvent = new ManualResetEvent(false);
        }

        bool ITestableConnection.WaitForAuthentication(int timeToWaitInMilliseconds)
        {
            return connectionEstablishedEvent.WaitOne(timeToWaitInMilliseconds);
        }

        bool ITestableConnection.WaitForMessageRecieved(int timeToWaitInMilliseconds)
        {
            return messageReceivedEvent.WaitOne(timeToWaitInMilliseconds);
        }

        void ITestableConnection.ResetMessageReceivedSemaphor()
        {
            messageReceivedEvent.Reset();
        }

        private Message lastMessage;
        Message ITestableConnection.LastMessage{get {return lastMessage;} }

        public void Open()
        {
            connectionEstablishedEvent.Reset();
            status = ConnectionStatus.Connecting;
            xmppCon.Open();
        }

        private void xmppCon_OnError(object sender, Exception ex)
        {
            throw new XMPPException("Error", ex);
        }

        private void xmppCon_OnRosterItem(object sender, RosterItem item)
        {
            roster.Add(item);
        }

        private void xmppCon_OnLogin(object sender)
        {
            //var iq = new IQ(IqType.get);
            //iq.Query = new Element("vCard xmlns='vcard-temp'");
            //xmppCon.Send(iq);
            status = ConnectionStatus.Authenticated;
            connectionEstablishedEvent.Set();
        }

        private void xmppCon_OnMessage(object sender, Message msg)
        {
            if(MessageReceived != null)
            {
                MessageReceived(this, new MessageEventArgs(msg));
            }
            lastMessage = msg;
            messageReceivedEvent.Set();
            //foreach (Gent gent in onlineGents)
            //{
            //    if (gent.IsThisJid(msg.From))
            //    {
            //        Conversation c = activeConversations.Find(conv => conv.Gent.Equals(gent));
            //        if (c != null)
            //        {
            //            c.GetNewMessage(msg);
            //        }
            //        else
            //        {
            //            Conversation conversation = new Conversation(gent, myVcard, xmppCon, activeConversations, onlineGents);
            //            if (!activeConversations.Contains(conversation))
            //            {
            //                activeConversations.Add(conversation);
            //                conversation.ShowWindow();
            //                conversation.GetNewMessage(msg);
            //            }
            //        }
            //        break;
            //    }
            //}
        }

        private void xmppCon_OnPresence(object sender, Presence pres)
        {
            foreach (RosterItem rosterItem in roster)
            {
                if (rosterItem.Jid.User.Equals(pres.From.User))
                {
                    var gent = new Gent(rosterItem, pres);
                    if (pres.Type == PresenceType.available)
                    {
                        var pres2 = new Presence {Type = PresenceType.available};
                        if (pres.Show == ShowType.NONE)
                        {
                            pres2.Show = ShowType.away;
                        }
                        else if (pres.Show == ShowType.away)
                        {
                            pres2.Show = ShowType.NONE;
                        }
                        onlineGents.Add(new Gent(rosterItem, pres2));


                        //foreach (Conversation conversation in
                        //            activeConversations.Where(conversation => conversation.Gent.Equals(gent)))
                        //{
                        //    Uri uri;
                        //    if (pres.Show == ShowType.NONE)
                        //    {
                        //        uri = new Uri("Iconography/onlineicon.png", UriKind.Relative);
                        //    }
                        //    else
                        //    {
                        //        uri = new Uri("Iconography/awayicon.png", UriKind.Relative);
                        //    }
                        //    Conversation conv = conversation;
                        //    Utilities.PerformActionInWpfThread(() => conv.SetWindowIcon(uri));
                        //}
                    }
                    else if (pres.Type == PresenceType.unavailable)
                    {
                        // remove from online gents
                    }
                    break;
                }
            }
        }

        private void xmppCon_OnRosterEnd(object sender)
        {
            xmppCon.Show = ShowType.NONE;
            xmppCon.SendMyPresence();
        }

        private void xmppCon_OnRosterStart(object sender)
        {}

        private void xmppCon_OnClose(object sender)
        {
            if (status != ConnectionStatus.Closed || status != ConnectionStatus.Closing)
            {
                //MessageBox.Show("Connection Dropped, Attempting to reopen");
                xmppCon.Open();
                return;
            }
            status = ConnectionStatus.Closed;
        }

        public void SendMessage(Jid to, string subject, string message)
        {
            var msg = new Message(to, jid, MessageType.chat, message, subject);
            xmppCon.Send(msg);
        }

        private void xmppCon_OnIq(object sender, IQ iq)
        {
            if (iq.Vcard != null)
            {
                myVcard = iq.Vcard;
            }
        }

        private void xmppCon_OnAuthError(object sender, Element e)
        {
            status = ConnectionStatus.AuthenticationFailed;
            connectionEstablishedEvent.Set();
            LastErrorMessage = e;
        }

        public  void Close()
        {
            status = ConnectionStatus.Closing;
            xmppCon.Close();
        }
    }

    
}