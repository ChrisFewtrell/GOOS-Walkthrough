using NUnit.Framework;

namespace GOOSTests
{
    /// <summary>
    /// You need to configure an XMPP login with name "auction-item-54321", password = "auction"
    /// </summary>
    [TestFixture]
    public class AuctionSniperEndToEndTest
    {
        private FakeAuctionServer auction;
        private ApplicationRunner application;

        [SetUp]
        public void SetUp()
        {
            auction = new FakeAuctionServer("item-54321");
            application = new ApplicationRunner();
        }

        [Test]
        public void SniperJoinsAuctionUntilAuctionCloses()
        {
            auction.StartSellingItem();
            application.StartBiddingIn(auction);
            auction.HasReceivedJoinRequestFromSniper();
            auction.AnnounceClosed();
            application.ShowsSniperHasLostAuction();
        }

        [TearDown]
        public void TearDown()
        {
            auction.Stop();
            application.Stop();
        }
    }
}
