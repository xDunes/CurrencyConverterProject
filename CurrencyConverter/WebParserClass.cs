using System;
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

/***********************************************************************************************
 * 		Course:			CMSC 495 6380
 * 		Assignment:		Final Project
 * 		Author:			Team A (Martynas Mickus, Nicholas Almiron, Lawrence Adams)
 * 		Date:			10/13/2013
 * 		Project:		Currency Converter
 ***********************************************************************************************/
namespace CurrencyConverter
{
    class WebParserClass
    {
        DatabaseClass clsDB;
        int dbStat = -1;
        Thread thread;
        //initialize the class
        public WebParserClass()
        {
            ServicePointManager.DefaultConnectionLimit = 10000; //this will allow extra connections so .NET library will have time to close previous and empty up the connection pool
            clsDB = new DatabaseClass(); //create database class for saving conversion rates
        }//WebParserClass

        //This function will parse a web page and retrieve all currency names
        public ArrayList getCurrencyNames()
        {
            //Regular expression to extract currency names from HTML
            Regex regexOPTION=new Regex("</?\\w+\\s+\\w+=\"(.*)\">(.*)</\\w+>");
		    ArrayList tempArray=new ArrayList();
            try
            {
                //get HTML from the site
                WebRequest request = WebRequest.Create("https://www.google.com/finance/converter");
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                StreamReader reader = new StreamReader(data);
                bool parse = false;
                //read one line at a time untill criteria is met.  Once met use regular expression to extract needed information
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
            catch { }//website not reachable 
            //If website parsing failed, fail over to Database
		    if (tempArray.Count==0){
                try
                {
                    tempArray = clsDB.getCurrencyNames();
                }
                catch { }//Database not reachable
		    }//if
		    return tempArray;
	    }//getCurrencyNames

        //start a second thread to get a list of conversion rates.  This will prevent GUI from locking up while rates are quaried
        public void getAllConversionRates(ArrayList alCurrencyNames){
            thread = new Thread(() => threadAllConversionRates(alCurrencyNames));
            thread.Start();
		}//getAllConversionRates
        
        //get rates all currencies
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
                //Regular expression to extract conversion rate from HTML
                Regex regexRate = new Regex("bld>([0-9]*\\.?[0-9]*)");
                WebRequest request = WebRequest.Create("https://www.google.com/finance/converter?a=1&from=" + ccFrom.getShortName() + "&to=" + ccTo.getShortName());
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                StreamReader reader = new StreamReader(data);
                //Read one line of HTML at a time until criteria is met then extract rate
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
            catch { } //website not reachable 
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

        //Initialize DB connection
        public int openDB()
        {
            dbStat = clsDB.InitDatabase();
            return dbStat;
        }//openDB

        //close DB connection
        public void closeDB()
        {
            thread.Abort();
            clsDB.CloseDBConn();
        }//closeDB

    }//WebParserClass
}//CurrencyConverter
