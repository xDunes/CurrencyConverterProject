using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
namespace CurrencyConverter
{
    public partial class frmMain : Form
    {
        private ArrayList alCurrencyNames;
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            WebParserClass webParser = new WebParserClass();
            alCurrencyNames = webParser.getCurrencyNames();
            updateComboBoxes();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {

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
