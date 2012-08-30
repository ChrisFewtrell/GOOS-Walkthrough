using System;
using agsXMPP;

namespace XmppWrapper
{
    /// <summary>
    /// An abstraction of the Jabber Identifier (Jid) 
    /// </summary>
    public sealed class Identifier
    {
        public Identifier(string user, string server, string resource)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (server == null)
            {
                throw new ArgumentNullException("server");
            }

            User = user;
            Server = server;
            Resource = resource;
        }

        public string User{get;private set;}

        public string Server{get;private set;}

        public string Resource{get;private set;}

        internal Jid ToJid()
        {
            return new Jid(User, Server, Resource);
        }

        internal static Identifier FromJid(Jid jid)
        {
            if(jid == null)
            {
                return null;
            }
            return new Identifier(jid.User, jid.Server, jid.Resource);
        }

        internal static Jid ToJid(Identifier id)
        {
            if(id == null)
            {
                return null;
            }
            return new Jid(id.User, id.Server, id.Resource);
        }
    }
}