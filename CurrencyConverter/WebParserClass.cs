﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

namespace CurrencyConverter
{
    class WebParserClass
    {
        DatabaseClass clsDB;
        Thread thread;
        public WebParserClass()
        {
            ServicePointManager.DefaultConnectionLimit = 10000;
            clsDB = new DatabaseClass();
        }//WebParserClass


        public ArrayList getCurrencyNames()
        {
            Regex regexOPTION=new Regex("</?\\w+\\s+\\w+=\"(.*)\">(.*)</\\w+>");
		    ArrayList tempArray=new ArrayList();
            try
            {
                
                WebRequest request = WebRequest.Create("https://www.google.com/finance/converter");
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                StreamReader reader = new StreamReader(data);
                bool parse = false;
                while (reader.Peek() >= 0)
                {
                    html = reader.ReadLine();
                    if (html.Contains("select name=from"))
                    {
                        parse = true;
                    }//if
                    else if (html.Contains("</select>") && parse)
                    {
                        parse = false;
                    }//elseif
                    else if (parse)
                    {
                        Match matchOption = regexOPTION.Match(html);
                        CurrencyClass currency = new CurrencyClass(matchOption.Groups[1].Value, matchOption.Groups[2].Value);
                        tempArray.Add(currency);
                    }//elseif
                }//while
            }//try
            catch { }
		    if (tempArray.Count==0){
    			tempArray = clsDB.getCurrencyNames();
		    }//if
		    return tempArray;
	    }//getCurrencyNames
        public void getAllConversionRates(ArrayList alCurrencyNames){
            thread = new Thread(() => threadAllConversionRates(alCurrencyNames));
            thread.Start();
		}//getAllConversionRates
        public int openDB()
        {
            int test = clsDB.InitDatabase();
            return test;
        }
        public void closeDB()
        {
            thread.Abort();
            clsDB.CloseDBConn();
        }
        private void threadAllConversionRates(ArrayList alCurrencyNames)
        {
            bool dbStatus = true;
            
            foreach (CurrencyClass currencyFrom in alCurrencyNames)
            {
                foreach (CurrencyClass currencyTo in alCurrencyNames)
                {
                    if (!currencyFrom.getShortName().Equals(currencyTo.getShortName()) && dbStatus)
                    {
                        RateClass rate = getSingleConversionRate(currencyFrom, currencyTo, false);
                        if (rate != null)
                        {
                            dbStatus = clsDB.saveRate(rate);
                        }//if
                    }//if
                }//foreach
            }//foreach
        }//threadAllConversionRates
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
                            Debug.WriteLine("Rate from " + ccFrom.getShortName() + " to " + ccTo.getShortName() + " is " + matchRate.Groups[1].Value);
                            rate = new RateClass(ccFrom, ccTo, Convert.ToDouble(matchRate.Groups[1].Value), DateTime.Now);
                        }//if
                    }//if
                }//while
            }//try
            catch { }
            if (useDB)
            {
                if (rate == null)
                {
                    rate = clsDB.getSingleConversionRate(ccFrom, ccTo);
                }//if
                else
                {
                    clsDB.saveRate(rate);
                }//else
            }//if
            return rate;
        }//getSingleConversionRate
    }//WebParserClass
}//CurrencyConverter
