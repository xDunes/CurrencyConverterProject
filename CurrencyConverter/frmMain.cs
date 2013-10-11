﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
namespace CurrencyConverter
{
    public partial class frmMain : Form
    {
        private ArrayList alCurrencyNames;
        private WebParserClass webParser;
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            webParser = new WebParserClass();
            alCurrencyNames = webParser.getCurrencyNames();
            if (alCurrencyNames.Count != 0)
            {
                updateComboBoxes();
                webParser.getAllConversionRates(alCurrencyNames);
            }
            else
            {
                MessageBox.Show("Error: Rates not currently available at this time.");
            }
            //webParser.queueAllConversionRates(alCurrencyNames);
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            CurrencyClass ccFrom = new CurrencyClass((string)((ComboboxItem)cmbFrom.SelectedItem).Value, ((ComboboxItem)cmbFrom.SelectedItem).Text);
            CurrencyClass ccTo = new CurrencyClass((string)((ComboboxItem)cmbTo.SelectedItem).Value, ((ComboboxItem)cmbTo.SelectedItem).Text);
            RateClass rate = webParser.getSingleConversionRate(ccFrom, ccTo, true);
            if (rate != null)
            {
                txtTo.Text = "" + (Convert.ToDouble(txtFrom.Text) * rate.getRate());
                lblAsOf.Text = "As Of " + rate.getTimeDate().ToString();
            }
            else
            {
                MessageBox.Show("ERROR: Could not retrieve rate for " + ccFrom.getShortName() + " to " + ccTo.getShortName() + " conversion rate at this time!");
            }
            
        }
        private void updateComboBoxes()
        {
            foreach (CurrencyClass currency in alCurrencyNames)
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = currency.getLongName();
                item.Value = currency.getShortName();
                cmbFrom.Items.Add(item);
                cmbTo.Items.Add(item);
                cmbFrom.SelectedIndex = 0;
                cmbTo.SelectedIndex = 0;
            }
        }
    }
}
