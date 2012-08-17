using agsXMPP.protocol.client;

namespace XmppWrapper
{
    public interface ITestableConnection : IConnection
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
}