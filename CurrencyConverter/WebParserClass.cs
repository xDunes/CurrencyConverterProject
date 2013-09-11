using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CurrencyConverter
{
    class WebParserClass
    {
        DatabaseClass clsDB;
        public WebParserClass()
        {
            clsDB = new DatabaseClass();
        }
        public ArrayList getCurrencyNames()
        {
		    ArrayList tempArray=new ArrayList();
		    if (tempArray.Count==0){
    			clsDB.getCurrencyNames();
		    }
		    return tempArray;
	    }
        public void getAllConversionRates(ArrayList alCurrencyNames){
            foreach (CurrencyClass currencyFrom in alCurrencyNames)
            {
                foreach (CurrencyClass currencyTo in alCurrencyNames)
                {
                    RateClass rate;
                    rate = getSingleConversionRate(currencyFrom, currencyTo);
                    if (rate != null)
                    {
                        clsDB.saveRate(rate);
                    }
                }
            }
		}
        public RateClass getSingleConversionRate(CurrencyClass ccFrom, CurrencyClass ccTo)
        {
            RateClass rate=null;
            return rate;
        }
    }
}
