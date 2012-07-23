using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkout
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() > 1)
            {
                Console.WriteLine(@"Usage: checkout.exe <items>
  <items> should be a string of SKUs" + Environment.NewLine);
            }

            var totalPriceForAllItems = new Checkout().GetTotal(args.Single());
            Console.WriteLine(string.Format("Total for items scanned: {0:C2}", totalPriceForAllItems));
        }
    }
}
