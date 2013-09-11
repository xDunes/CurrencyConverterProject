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
    }
}
