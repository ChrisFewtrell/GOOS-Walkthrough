using NUnit.Framework;
using XmppWrapper;
using Message = XmppWrapper.Message;

namespace XmppWrapperTests
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
            Assert.That(waitForAuthentication, Is.True, "Did not authenticate within given time span... are you running a server?  Have you added " + ValidUserName + " as a user??");
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
        public void TestSendingMessage_ToYourSelf_ShouldCauseLastMessageToBeSet()
        {
            ITestableConnection conn = new XmppConnectionWrapper(ValidUserName, ValidPassword, Server);
            conn.Open();
            WaitForAuthentication(conn);

            conn.SendMessage(new Identifier(ValidUserName, Server, null), "donkey", "hello");
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

            conn.SendMessage(new Identifier(ValidUserName, Server, null), "donkey", "hello");
            WaitForMessageReceived(conn);

            Assert.That(lastMessage.Body, Is.EqualTo("hello"));
            Assert.That(lastMessage.Subject, Is.EqualTo("donkey"));
        }

    }
}