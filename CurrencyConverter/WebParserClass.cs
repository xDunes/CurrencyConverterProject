using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace CurrencyConverter
{
    class WebParserClass
    {
        DatabaseClass clsDB;
        public WebParserClass()
        {
            ServicePointManager.DefaultConnectionLimit = 10000;
            clsDB = new DatabaseClass();
        }//WebParserClass
        public ArrayList getCurrencyNames()
        {
            Regex regexOPTION=new Regex("</?\\w+\\s+\\w+=\"(.*)\">(.*)</\\w+>");
		    ArrayList tempArray=new ArrayList();
            WebRequest request = WebRequest.Create("https://www.google.com/finance/converter");
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string html = String.Empty;
            StreamReader reader= new StreamReader(data);
            bool parse = false;
            while (reader.Peek() >= 0)
            {
                html = reader.ReadLine();
                if (html.Contains("select name=from"))
                {
                    parse = true;
                }
                else if (html.Contains("</select>") && parse)
                {
                    parse = false;
                }
                else if (parse)
                {
                    Match matchOption = regexOPTION.Match(html);
                    CurrencyClass currency = new CurrencyClass(matchOption.Groups[1].Value,matchOption.Groups[2].Value);
                    tempArray.Add(currency);
                }
            }
		    if (tempArray.Count==0){
    			clsDB.getCurrencyNames();
		    }
		    return tempArray;
	    }
        public void getAllConversionRates(ArrayList alCurrencyNames){
            Thread thread = new Thread(() => threadAllConversionRates(alCurrencyNames));
            thread.Start();
		}
        private void threadAllConversionRates(ArrayList alCurrencyNames)
        {
            foreach (CurrencyClass currencyFrom in alCurrencyNames)
            {
                    foreach (CurrencyClass currencyTo in alCurrencyNames)
                    {
                        if (!currencyFrom.getShortName().Equals(currencyTo.getShortName()))
                        {
                            RateClass rate = getSingleConversionRate(currencyFrom, currencyTo, false);
                            if (rate != null)
                            {
                                //Marty, I added this because we need to have error checking in case SaveRate can't access the database for any reason. We can discuss tommorrow or change back if needed
                                //****************************************************************************************************
                                bool status = clsDB.saveRate(rate);
                                if (status == false)
                                {
                                    //database class will return false if errors occured. Need to handle here
                                }
                                //****************************************************************************************************
                                //We will need to decide how we are going to handle a 'false' return. Obviously we dont want it to keep iterating through the loop so we will have to break out and return an 
                                //error of some sort. Again, we can discuss tommorrow or change back if needed
                            }
                        }
                    }
            }
            
        }
        public RateClass getSingleConversionRate(CurrencyClass ccFrom, CurrencyClass ccTo, bool useDB)
        {
            RateClass rate=null;
            try
            {
                Regex regexRate = new Regex("bld>([0-9]*\\.?[0-9]*)");
                WebRequest request = WebRequest.Create("https://www.google.com/finance/converter?a=1&from=" + ccFrom.getShortName() + "&to=" + ccTo.getShortName());
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                StreamReader reader = new StreamReader(data);
                while (reader.Peek() >= 0)
                {
                    html = reader.ReadLine();
                    if (html.Contains("span class=bld"))
                    {
                        Match matchRate = regexRate.Match(html);
                        if (matchRate.Success)
                        {
                            //Debug.WriteLine("Rate from " + ccFrom.getShortName() + " to " + ccTo.getShortName() + " is " + matchRate.Groups[1].Value);
                            rate = new RateClass(ccFrom, ccTo, Convert.ToDouble(matchRate.Groups[1].Value), DateTime.Now);
                        }
                    }
                }
            }
            catch { }

            if (rate == null && useDB)
            {
                rate = clsDB.getSingleConversionRate(ccFrom, ccTo);
            }
            return rate;
        }
    }
}
