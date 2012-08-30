namespace XmppWrapper
{
    public class ConnectionFactory
    {
         public ITestableConnection CreateConnection(string userName, string password, string server)
         {
             return new XmppConnectionWrapper(userName, password, server);
         }
    }
}