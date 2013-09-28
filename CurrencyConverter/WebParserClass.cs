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
            foreach (CurrencyClass currencyFrom in alCurrencyNames)
            {
                foreach (CurrencyClass currencyTo in alCurrencyNames)
                {
                    RateClass rate = getSingleConversionRate(currencyFrom, currencyTo);
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
            Regex regexRate = new Regex("[0-9]*\\.[0-9]*");
            Debug.WriteLine("Creating request");
            WebRequest request = WebRequest.Create("https://www.google.com/finance/converter?a=1&from=" + ccFrom.getShortName() + "&to=" + ccTo.getShortName());
            Debug.WriteLine("Getting Response");
            WebResponse response = request.GetResponse();
            Debug.WriteLine("Setting up a stream");
            Stream data = response.GetResponseStream();
            string html = String.Empty;
            Debug.WriteLine("Setting up a reader");
            StreamReader reader = new StreamReader(data);
            Debug.WriteLine("Start Parsing!");
            while (reader.Peek() >= 0)
            {
                html = reader.ReadLine();
                if (html.Contains("span class=bld"))
                {
                    //Debug.WriteLine(html);
                    Match matchRate = regexRate.Match(html);
                    //Debug.WriteLine("Found span: " + matchRate.Groups[0].Value);
                    rate = new RateClass(ccFrom,ccTo,Convert.ToDouble(matchRate.Groups[0].Value),DateTime.Now);
                }
            }
            Debug.WriteLine("Finished Parsing!");
            return rate;
        }
    }
}
