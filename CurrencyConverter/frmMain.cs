using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace CurrencyConverter
{
    public partial class frmMain : Form
    {
        private ArrayList alCurrencyNames;  //will store currency names
        private WebParserClass webParser; //used for retriecing rates and currency names

        //form constructor
        public frmMain()
        {
            InitializeComponent();
        }//frmMain

        //when form loads declare variables, start gathering all conversion rates, get currency names
        private void frmMain_Load(object sender, EventArgs e)
        {
            webParser = new WebParserClass();
            int dbStatus = webParser.openDB();
            alCurrencyNames = webParser.getCurrencyNames();
            if (alCurrencyNames.Count != 0)
            {
                updateComboBoxes();
                webParser.getAllConversionRates(alCurrencyNames);
            }//if
            else
            {
                MessageBox.Show("Could not retrieve currency names.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }//else
        }//frmMain_Load

        //Converts currencies when the button is clicked
        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (cmbFrom.SelectedItem == cmbTo.SelectedItem)
            {
                txtTo.Text = txtFrom.Text;
                lblAsOf.Text = "As Of " + DateTime.Now;
            }//if
            else
            {
                CurrencyClass ccFrom = new CurrencyClass((string)((ComboboxItem)cmbFrom.SelectedItem).Value, ((ComboboxItem)cmbFrom.SelectedItem).Text);
                CurrencyClass ccTo = new CurrencyClass((string)((ComboboxItem)cmbTo.SelectedItem).Value, ((ComboboxItem)cmbTo.SelectedItem).Text);
                RateClass rate = webParser.getSingleConversionRate(ccFrom, ccTo, true);
                Debug.WriteLine(rate.ToString());
                if (rate != null)
                {
                    txtTo.Text = "" + (Convert.ToDouble(txtFrom.Text) * rate.getRate());
                    lblAsOf.Text = "As Of " + rate.getTimeDate().ToString();
                }//if
                else
                {
                    MessageBox.Show("Could not retrieve conversion rate for " + ccFrom.getShortName() + " to " + ccTo.getShortName() + " at this time!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }//else
            }//else
        }//btnConvert_Click

        //loads currency names in to the combo boxes
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
            }//foreach
        }//updateComboBoxes

        //when form is closed, kill DB connection, kill thread
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            webParser.closeDB();
        }//frmMain_FormClosed

        //function to prevent bad user input
        private void txtFrom_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }//if

            // only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }//if
        }//updateComboBoxes
    }//frmMain
}//namespace
