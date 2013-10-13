using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/***********************************************************************************************
 * 		Course:			CMSC 495 6380
 * 		Assignment:		Final Project
 * 		Author:			Team A (Martynas Mickus, Nicholas Almiron, Lawrence Adams)
 * 		Date:			10/13/2013
 * 		Project:		Currency Converter
 ***********************************************************************************************/
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
        }//RateClass
        public CurrencyClass getFrom(){
		    return ccFrom;
	    }//getFrom
	    public CurrencyClass getTo(){
		    return ccTo;
	    }//getTo
	    public double getRate(){
		    return dblRate;
	    }//getRate
	    public DateTime getTimeDate(){
            return dtDateTime;
	    }//getTimeDate
        public override string ToString()
        {
            return "From: " + ccFrom.ToString() + "  To: " + ccTo.ToString() + "  Rate: " + dblRate + "  Date: " + dtDateTime.ToString();
        }//ToString
    }
}//CurrencyConverter
