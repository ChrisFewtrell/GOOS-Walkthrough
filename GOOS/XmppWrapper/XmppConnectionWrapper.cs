using System;
using System.Collections.Generic;
using System.Threading;
using GOOSTests;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.vcard;

namespace XmppWrapper
{
    public class XmppConnectionWrapper : ITestableConnection
    {
        private readonly XmppClientConnection xmppCon;
        private readonly Jid jid;
        private Vcard myVcard;

        private readonly List<RosterItem> roster = new List<RosterItem>();

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

        void ITestableConnection.ResetMessageReceivedSemaphore()
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
        }

        private void xmppCon_OnPresence(object sender, Presence pres)
        {
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