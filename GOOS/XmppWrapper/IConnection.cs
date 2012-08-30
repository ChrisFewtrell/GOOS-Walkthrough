using System;

namespace XmppWrapper
{
    public interface IConnection
    {
        ConnectionStatus Status{get;}
        void Open();
        void SendMessage(Identifier to, string subject, string message);
        void Close();
        event EventHandler<MessageEventArgs> MessageReceived;
    }
}