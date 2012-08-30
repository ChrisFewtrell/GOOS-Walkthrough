using System;
using agsXMPP.protocol.client;

namespace XmppWrapper
{
    public interface ITestableConnection : IConnection
    {
        /// <summary>
        /// Waits for the connection to either succeed or fail to authenticate
        /// <see cref="ConnectionStatus.Authenticated"/>,
        /// </summary>
        bool WaitForAuthentication(TimeSpan timeSpan);
        /// <summary>
        /// Waits for a message to be received.
        /// </summary>
        /// <param name="timeToWaitInMilliseconds"></param>
        /// <returns></returns>
        bool WaitForMessageRecieved(TimeSpan timeSpan);

        void ResetMessageReceivedSemaphore();

        Message LastMessage{get;}
    }
}