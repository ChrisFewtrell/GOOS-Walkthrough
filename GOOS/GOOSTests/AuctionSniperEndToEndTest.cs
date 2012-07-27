﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace GOOSTests
{
    [TestFixture]
    public class AuctionSniperEndToEndTest
    {
        private  FakeAuctionServer auction;
        private  ApplicationRunner application;

        [SetUp]
        public void SetUp()
        {
            auction = new FakeAuctionServer("item-54321");
            application = new ApplicationRunner();
        }

        [Test]
        public void SinperJoinsAuctionUntilAuctionCloses()
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

    internal class ApplicationRunner
    {
        public void StartBiddingIn(FakeAuctionServer auction)
        {
            throw new NotImplementedException();
        }

        public void ShowsSniperHasLostAuction()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

    internal class FakeAuctionServer
    {
        public FakeAuctionServer(string item)
        {
            throw new NotImplementedException();
        }

        public void StartSellingItem()
        {
            throw new NotImplementedException();
        }

        public void HasReceivedJoinRequestFromSniper()
        {
            throw new NotImplementedException();
        }

        public void AnnounceClosed()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}