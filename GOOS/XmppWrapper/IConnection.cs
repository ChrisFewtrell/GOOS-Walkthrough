using System;
using agsXMPP;

namespace XmppWrapper
{
    public interface IConnection
    {
        ConnectionStatus Status{get;}
        void Open();
        void SendMessage(Jid to, string subject, string message);
        void Close();
        event EventHandler<MessageEventArgs> MessageReceived;
    }
}