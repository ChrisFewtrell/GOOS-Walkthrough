using System;
using agsXMPP.protocol.client;

namespace XmppWrapper
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message{get;private set;}

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}