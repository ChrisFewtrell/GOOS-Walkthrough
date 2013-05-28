using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Checkout.Tests
{
    [TestFixture]
    public class CheckoutTests
    {
        [Test]
        public void GetTotal_ShouldReturn0_WhenNoItemsAreScanned()
        {
            var checkout = new Checkout();


            var totalPrice = checkout.GetTotal(string.Empty);

            Assert.That(totalPrice, Is.EqualTo(0));
        }

        [Test]
        public void GetTotal_ShouldReturn50_WhenItemIsScanned()
        {

            var checkout = new Checkout();


            var scannedItemPrice = checkout.GetTotal("A");

            Assert.That(scannedItemPrice, Is.EqualTo(50));
        
        }

        [Test]
        public void GetTotal_ShouldReturn80_WhenItemScanned()
        {
               var checkout = new Checkout();


            var scannedItemPrice = checkout.GetTotal("AB");

            Assert.That(scannedItemPrice, Is.EqualTo(80));
        }

        [Test]
        public void GetTotal_ShouldReturn115_WhenItemScanned()
        {
            var checkout = new Checkout();


            var scannedItemPrice = checkout.GetTotal("ABCD");

            Assert.That(scannedItemPrice, Is.EqualTo(115));
        }

        [Test]
        public void GetTotal_ShouldReturn115_WhenBadItemScanned()
        {
            var checkout = new Checkout();


            var scannedItemPrice = checkout.GetTotal("ABCDE");

            Assert.That(scannedItemPrice, Is.EqualTo(115));
        }
    }
}
