using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Data.OleDb;
using ADOX;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;


namespace CurrencyConverter
{
    class DatabaseClass
    {

        private OleDbConnection MyConn;
        static private string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CurrencyConverter";
        private string path = ProgramData + "\\CurrencyDB.accdb";//add database path later
        public bool saveRate(RateClass rate)
        {
            MessageBox.Show("saveRate started!");
            bool Successful = false;
            CurrencyClass ccFrom = rate.getFrom();
            CurrencyClass ccTo = rate.getTo();
            int Status = InitDatabase();
            switch (Status)
            {
                case 0: case 1: case 2:
                    {
                        //Sucess
                        if (checkForDuplicate(rate) == false)
                        {
                            try
                            {
                                string insert = "INSERT INTO CurrencyConverter ([CurFromLong], [CurFromShort], [CurToLong], [CurToShort], [Rate], [DateTime]) VALUES (@CurFromLong, @CurFromShort, @CurToLong, @CurToShort, @Rate, @DateTime);";
                                OleDbCommand cmd = new OleDbCommand(insert, MyConn);
                                cmd.Parameters.AddWithValue("@CurFromLong", ccFrom.getLongName());
                                cmd.Parameters.AddWithValue("@CurFromShort", ccFrom.getShortName());
                                cmd.Parameters.AddWithValue("@CurToLong", ccTo.getLongName());
                                cmd.Parameters.AddWithValue("@CurToShort", ccTo.getShortName());
                                cmd.Parameters.AddWithValue("@Rate", rate.getRate().ToString());
                                cmd.Parameters.AddWithValue("@DateTime", rate.getTimeDate().ToString());
                                cmd.ExecuteNonQuery();
                                Successful = true;
                                MyConn.Close();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                                //returns false
                            }
                        }
                        else
                        {
                            try
                            {
                                string update = "UPDATE CurrencyConverter SET [Rate]=@Rate, [DateTime]=@DateTime WHERE CurFromShort LIKE '" + ccFrom.getShortName() + "' AND CurToShort LIKE '" + ccTo.getShortName() + "'";
                                OleDbCommand cmd = new OleDbCommand(update, MyConn);
                                cmd.Parameters.AddWithValue("@Rate", rate.getRate().ToString());
                                cmd.Parameters.AddWithValue("@DateTime", rate.getTimeDate().ToString());
                                cmd.ExecuteNonQuery();
                                Successful = true;
                                MyConn.Close();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                                //returns false
                            }
                        }
                        break;
                    }
                case 3: case 4: case 5:
                    {
                        //Errors. returns false
                        break;
                    }
                default:
                    {
                        //Unexpected error. Returns false
                        break;
                    }
            }
            MessageBox.Show("saveRate Finished!");
            return Successful;
        }
        public RateClass getSingleConversionRate(CurrencyClass ccFrom, CurrencyClass ccTo)
        {
            RateClass rate = null;
            int Status = InitDatabase();
            switch (Status)
            {
                case 0: case 1: case 2:
                    {
                        //Sucess
                        string CommandString = "Select * from CurrencyConverter where CurFromShort like '" + ccFrom.getShortName() + "' and CurToShort like '" + ccTo.getShortName() + "'";
                        OleDbCommand cmd = new OleDbCommand(CommandString, MyConn);
                        OleDbDataReader reader = cmd.ExecuteReader();
                        if (reader.RecordsAffected > 1)
                        {
                            //more then one record was returned
                        }
                        else
                        {
                            double rateVal = 0;
                            DateTime dateAdded = DateTime.Now; 
                            while (reader.Read())
                            {
                                rateVal = Convert.ToDouble(reader["Rate"].ToString());
                                dateAdded = Convert.ToDateTime(reader["DateTime"].ToString());
                            }
                            rate = new RateClass(ccFrom, ccTo, rateVal, dateAdded);
                        }
                        MyConn.Close();
                        break;
                    }
                case 3: case 4: case 5:
                    {
                        //Errors. returns null
                        break;
                    }
                default:
                    {
                        //Unexpected error. Return null
                        break;
                    }
            }
            return rate;
        }
        public ArrayList getCurrencyNames()
        {
            ArrayList tempArray = new ArrayList();
            int Status = InitDatabase();
            switch (Status)
            {
                case 0: case 1: case 2:
                    {
                        //Sucess
                        string CommandString = "Select * from CurrencyConverter";
                        OleDbCommand cmd = new OleDbCommand(CommandString, MyConn);
                        OleDbDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            CurrencyClass ccFrom = new CurrencyClass(reader["CurFromShort"].ToString(), reader["CurFromLong"].ToString());
                            CurrencyClass ccTo = new CurrencyClass(reader["CurToShort"].ToString(), reader["CurToLong"].ToString());
                            if (!tempArray.Contains(ccFrom))
                            {
                                tempArray.Add(ccFrom);
                            }
                            if (!tempArray.Contains(ccTo))
                            {
                                tempArray.Add(ccTo);
                            } 
                        }

                        MyConn.Close();
                        break;
                    }
                case 3: case 4: case 5:
                    {
                        //Errors. returns Empty ArrayList
                        break;
                    }
                default:
                    {
                        //Unexpected error. Returns Empty ArrayList
                        break;
                    }
            }
            Debug.WriteLine(tempArray.Count);
            return tempArray;
        }

        //Check for database and attempts to connect to it. Returns int to define status. Creates database if non-existant
        /* Return Codes 
         * 
         * Success Codes
         * 0 = Database found and connected
         * 1 = Database successfully created and connected
         * 2 = Database corrupt, Created new one and connected
         * 
         * Failure Codes
         * 3 = Database not found or corrupt. Unable to create new Database
         * 4 = Database is corrupt, unable to delete old Database
         * 5 = New Database is created, but unable to connect
         * */
        private int InitDatabase()
        {
            MessageBox.Show("InitDatabase Started!");
            bool ConnStatus = false; //true if connected, false if connection failed
            int returnCode = 0;
            String ConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";"; 
            //Check to see if database file exists
            
            if (File.Exists(path))
            {
                ConnStatus = OpenDatabaseConn(ConnString);
                if (ConnStatus == true)
                {
                    returnCode = 0;
                }
                else //Database is corrupt. Attempt to re-create
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        
                        returnCode = 4;
                    }
                    if (returnCode != 4)
                    {
                        //Create new database and attempt to connect
                        bool Created = CreateDatabase();
                        if (Created == true)
                        {
                            ConnStatus = OpenDatabaseConn(ConnString);
                            if (ConnStatus == true)
                            {
                                returnCode = 2;
                            }
                            else returnCode = 5;
                        }
                        else
                        {
                            returnCode = 3;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Should call create database now!");
                bool blCreated=false;
                try
                {

                    
                    blCreated = CreateDatabase();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                MessageBox.Show("Done calling Create Database Now!");
                if (blCreated)
                {
                    ConnStatus = OpenDatabaseConn(ConnString);
                    if (ConnStatus == true)
                    {
                        returnCode = 1;
                    }
                    else returnCode = 4;
                }
                else
                {
                    returnCode = 3;
                }
            }
            MessageBox.Show("InitDatabase Finished!");
            return returnCode;
        }
        //Creates Database
        private bool CreateDatabase()
        {
            MessageBox.Show("Create Database Started!");
            
            if (!Directory.Exists(ProgramData))
            {
                try {
                    Directory.CreateDirectory(ProgramData);
                }
                catch (Exception ex)
                { 
                    MessageBox.Show(ex.Message);
                }
            }

            
            //Code to create Database
            
            Table table = new Table();
            table.Name = "CurrencyConverter";
            table.Columns.Append("CurFromLong");
            table.Columns.Append("CurFromShort");
            table.Columns.Append("CurToLong");
            table.Columns.Append("CurToShort");
            table.Columns.Append("Rate");
            table.Columns.Append("DateTime");

            try
            {
                string CreateString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + "Jet OLEDB:Engine Type=5";
                Catalog cat = new Catalog();
                cat.Create(CreateString);
                cat.Tables.Append(table);

                //Database Created, Closing the database
                MyConn = cat.ActiveConnection as OleDbConnection;
                if (MyConn != null)
                {
                    MyConn.Close();
                }
                MessageBox.Show("Create Database Finished!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                MessageBox.Show("Create Database Finished! (failed)");
                return false;
            }
            
        }
        //Attempts to open database connection. Returns true if successful. 
        private bool OpenDatabaseConn(string ConnString)
        {
            try
            {
                MyConn = new OleDbConnection(ConnString);
                MyConn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                //Exception caught. Failed to connect to database
                return false;
            }
        }
        private bool checkForDuplicate(RateClass rate)
        {
            CurrencyClass ccFrom = rate.getFrom();
            CurrencyClass ccTo = rate.getTo();
            string CommandString = "Select * from CurrencyConverter where CurFromShort like '" + ccFrom.getShortName() + "' and CurToShort like '" + ccTo.getShortName() + "'";
            OleDbCommand cmd = new OleDbCommand(CommandString, MyConn);
            OleDbDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
