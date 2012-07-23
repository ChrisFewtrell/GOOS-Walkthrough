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
    }
}
