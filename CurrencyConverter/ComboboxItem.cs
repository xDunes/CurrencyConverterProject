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
    class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }//ToString
    }//ComboboxItem
}//CurrencyConverter
