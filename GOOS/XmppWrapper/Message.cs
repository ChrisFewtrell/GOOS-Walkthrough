namespace XmppWrapper
{
    public class Message
    {
        public Identifier From{get;private set;}

        public Identifier To{get;private set;}

        public string Body{get;private set;}

        public string Subject{get;private set;}

        public Message(Identifier @from, Identifier to, string body, string subject)
        {
            From = @from;
            To = to;
            Body = body;
            Subject = subject;
        }

        internal static Message FromMessage(agsXMPP.protocol.client.Message message)
        {
            return new Message(Identifier.FromJid(message.From), Identifier.FromJid(message.To), message.Body, message.Subject);
        }
    }
}
