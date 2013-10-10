using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CurrencyConverter
{
    class DatabaseClass
    {
        public void saveRate(RateClass rate)
        {
        }
        public void saveAllRates(ArrayList rateList)
        {
        }
        public RateClass getSingleConversionRate(CurrencyClass ccFrom, CurrencyClass ccTo)
        {
            RateClass rate = null;
            return rate;
        }
        public ArrayList getCurrencyNames()
        {
            ArrayList tempArray = new ArrayList();
            return tempArray;
        }
        private bool DatabaseConn(string path)
        {
            return true;
        }
    }
}
