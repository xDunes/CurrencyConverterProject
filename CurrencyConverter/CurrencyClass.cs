using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        public String getShortName()
        {
            return strShortName;
        }
        public String getLongName()
        {
            return strLongName;
        }
        public override bool Equals(object o)
        {
            if (o == null || o.GetType() != typeof(CurrencyClass))
                return false;
            CurrencyClass currency = (CurrencyClass)o;
            return (currency.getLongName().Equals(this.strLongName) && currency.getShortName().Equals(this.strShortName));
        }
        public override int GetHashCode()
        {
            return (strLongName + strShortName).GetHashCode();
        }
    }
}
