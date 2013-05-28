using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkout
{
    public class Checkout
    {
        Dictionary<char, double> priceList;
        public Checkout()
        {
            priceList = new Dictionary<char, double>();
            priceList.Add('A', 50);
            priceList.Add('B', 30);
            priceList.Add('C', 20);
            priceList.Add('D', 15);
        }
        public double GetTotal(string skus)
        {
            double result;
            double total = 0;
            int length = skus.Length;
            for (int i = 0; i < length; i++)
            {
                if(priceList.TryGetValue(skus[i],out result))
                    total += result;
            
                /*if (skus[i] == 'A')
                    total += 50;
                else if (skus[i] == 'B')
                    total += 30;
                else if (skus[i] == 'C')
                    total += 20;
                else if (skus[i] == 'D')
                    total += 15;*/
            }
            //throw new NotImplementedException();
            return total;
        }
    }
}
