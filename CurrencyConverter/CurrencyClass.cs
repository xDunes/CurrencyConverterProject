using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CurrencyConverter
{
    class CurrencyClass
    {
        private String strShortName;
	    private String strLongName;

        public CurrencyClass(String shortName, String longName)
        {
            strShortName = shortName;
            strLongName = longName;
        }//CurrencyClass
        public String getShortName()
        {
            return strShortName;
        }//getShortName
        public String getLongName()
        {
            return strLongName;
        }//getLongName
        public override bool Equals(object o)
        {
            if (o == null || o.GetType() != typeof(CurrencyClass))
                return false;
            CurrencyClass currency = (CurrencyClass)o;
            return (currency.getLongName().Equals(this.strLongName) && currency.getShortName().Equals(this.strShortName));
        }//Equals
        public override int GetHashCode()
        {
            return (strLongName + strShortName).GetHashCode();
        }//GetHashCode
        public string ToString()
        {
            string returnString = "ShortName: " + strShortName + "  LongName: " + strLongName;
            return returnString;
        }//ToString
    }//CurrencyClass
}//CurrencyConverter
