using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mime;
using GOOSTests;
using NUnit.Framework;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.vcard;
using Uri = System.Uri;

namespace GOOS
{
  
    /// <summary>
    /// This code is a copy and paste of Jonny's code with stuff commented out.
    /// </summary>
    public class XmppStrawGrasp
    {
        private readonly XmppClientConnection xmppCon;
        private Jid jid;
        private Vcard myVcard;

        private readonly List<RosterItem> roster = new List<RosterItem>();
        private readonly ObservableCollection<Gent> onlineGents = new ObservableCollection<Gent>();
        //private readonly List<Conversation> activeConversations = new List<Conversation>();

        private bool loginError;
        private bool loggingOut;

        public XmppStrawGrasp()
        {
            xmppCon = new XmppClientConnection();
        }

        private void xmppCon_OnError(object sender, Exception ex)
        {
            throw new Exception("Arrr error");
        }

        private void xmppCon_OnRosterItem(object sender, RosterItem item)
        {
            roster.Add(item);
        }

        private void xmppCon_OnLogin(object sender)
        {
            loggingOut = false;

            var iq = new IQ(IqType.get);
            iq.Query = new Element("vCard xmlns='vcard-temp'");
            xmppCon.Send(iq);
        }

        private void xmppCon_OnMessage(object sender, Message msg)
        {
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

        //private void listBoxItem_DoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var listBoxItem = (ListBoxItem) sender;
        //    var gent = listBoxItem.Content as Gent;

        //    Conversation c = activeConversations.Find(conv => conv.Gent.Equals(gent));
        //    if (c != null)
        //    {
        //        // set this event as done to return focus back to main window now
        //        e.Handled = true;
        //        c.ActivateWindow();
        //    }
        //    else
        //    {
        //        var conversation = new Conversation(gent, myVcard, xmppCon, activeConversations, onlineGents);
        //        activeConversations.Add(conversation);
        //        // set this event as done to return focus back to main window now
        //        e.Handled = true;
        //        conversation.ShowWindow();
        //    }
        //}

        private void xmppCon_OnRosterStart(object sender)
        {
            //Utilities.PerformActionInWpfThread(() => statustext.Text = "getting roster...");
        }

        private void xmppCon_OnClose(object sender)
        {
            if (!loggingOut)
            {
                //MessageBox.Show("Connection Dropped, Attempting to reopen");
                xmppCon.Open();
            }
        }

        public void SendMessage()
        {
        }

        public void Login(string userName, string password)
        {
            loginError = false;

            jid = new Jid(userName + "@localhost");

            xmppCon.Password = password;
            xmppCon.Username = jid.User;
            xmppCon.Server = jid.Server;
            xmppCon.AutoAgents = false;
            xmppCon.AutoPresence = true;
            xmppCon.AutoRoster = true;
            xmppCon.AutoResolveConnectServer = true;

            try
            {
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

                xmppCon.Open();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private void xmppCon_OnIq(object sender, IQ iq)
        {
            if (iq.Vcard != null)
            {
                myVcard = iq.Vcard;
                //Utilities.PerformActionInWpfThread(() => UsernameLabel.Text = iq.Vcard.Fullname);
                //MediaTypeNames.Image img = iq.Vcard.Photo.Image;
            }
        }

        private void xmppCon_OnAuthError(object sender, Element e)
        {
            if (!loginError)
            {
                throw new Exception("Arrrhhh no one loves me");
                loginError = true;
            }
        }

        public  void Logout()
        {
            loggingOut = true;
            xmppCon.Close();
        }
    }


    public class Gent : IComparable<Gent>
    {
        private RosterItem rosterEntry;
        private Presence presence;

        public RosterItem RosterEntry{get {return rosterEntry;}set {rosterEntry = value;}}

        public Presence RosterPresence{get {return presence;}set {presence = value;}}

        public string PresenceShow
        {
            get
            {
                switch (presence.Show)
                {
                    case ShowType.NONE:
                        return "Iconography/online.png";
                    case ShowType.away:
                        return "Iconography/away.png";
                }
                return "";
            }
        }

        public string UserName{get {return rosterEntry.Name;}}

        public Gent(RosterItem rosterEntry, Presence presence)
        {
            this.rosterEntry = rosterEntry;
            this.presence = presence;
        }

        public bool IsThisJid(Jid jid)
        {
            return rosterEntry.Jid.Equals(jid);
        }

        public int CompareTo(Gent other)
        {
            if (RosterPresence.Show == ShowType.NONE
                && other.RosterPresence.Show == ShowType.away)
            {
                return -1;
            }
            if ((RosterPresence.Show == ShowType.away
                 && other.RosterPresence.Show == ShowType.NONE))
            {
                return 1;
            }
            return UserName.CompareTo(other.UserName);
        }

        public override string ToString()
        {
            switch (presence.Show)
            {
                case ShowType.NONE:
                    return rosterEntry.Name + " (online)";
                case ShowType.away:
                    return rosterEntry.Name + " (away)";
            }
            return "";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != typeof(Gent))
            {
                return false;
            }
            var other = (Gent) obj;
            return rosterEntry.Jid.Equals(other.rosterEntry.Jid);
        }

        public override int GetHashCode()
        {
            return 2887 ^ rosterEntry.GetHashCode();
        }
    }
}
