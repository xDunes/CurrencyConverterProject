using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    class RateClass
    {
        private CurrencyClass ccFrom;
        private CurrencyClass ccTo;
        private double dblRate;
        private DateTime dtDateTime;
        public RateClass(CurrencyClass from, CurrencyClass to, double rate, DateTime dt)
        {
            ccFrom = from;
            ccTo = to;
            dblRate = rate;
            dtDateTime = dt;
        }
        public CurrencyClass getFrom(){
		    return ccFrom;
	    }
	    public CurrencyClass getTo(){
		    return ccTo;
	    }
	    public double getRate(){
		    return dblRate;
	    }
	    public DateTime getTimeDate(){
            return dtDateTime;
	    }
    }
}
