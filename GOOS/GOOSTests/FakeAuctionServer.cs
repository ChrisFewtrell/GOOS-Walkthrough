using System;
using System.Threading;
using XmppWrapper;

namespace GOOSTests
{
    internal class FakeAuctionServer
    {
        public const string ItemIdAsLogin = "auction-{0}";
        public const string AuctionResource = "Auction";
        public const string XmppHostname = "localhost";
        public const string AuctionPassword = "auction";

        private readonly string itemId;
        private ITestableConnection connection;
        private readonly SingleMessageListener messageListener = new SingleMessageListener();

        private readonly ManualResetEvent loginEvent = new ManualResetEvent(false);

        public FakeAuctionServer(string item)
        {
            this.itemId = item;
        }

        private string MakeLoginName()
        {
            return string.Format(ItemIdAsLogin, itemId);
        }

        public void StartSellingItem()
        {
            connection = new ConnectionFactory().CreateConnection(MakeLoginName(), AuctionPassword, XmppHostname);
            connection.Open();
        }

        public void HasReceivedJoinRequestFromSniper()
        {
            messageListener.ReceivesAMessage();
            // Could also check that connection.LastMessage is not null
        }

        public void AnnounceClosed()
        {
            //currentChat.sendMessage(new Message());
            connection.SendMessage(null, null, null);
        }

        public void Stop()
        {
            connection.Close();
        }

        public string GetItemId()
        {
            return itemId;
        }
    }
}