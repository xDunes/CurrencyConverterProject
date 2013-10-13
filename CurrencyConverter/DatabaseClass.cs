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
using System.Windows.Forms;


namespace CurrencyConverter
{
    class DatabaseClass
    {

        private OleDbConnection MyConn;//global var for database connection
        static private string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CurrencyConverter"; //Get path for ProgramDatabase
        private string path = ProgramData + "\\CurrencyDB.accdb";

        //Method used for saving rates into the database
        public bool saveRate(RateClass rate)
        {
            bool Successful = false;
            CurrencyClass ccFrom = rate.getFrom();
            CurrencyClass ccTo = rate.getTo();
                        //If entry does not exist insert it
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
                    //Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                    //returns false
                }
            }
            //if entry does exist, update it
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
                    MyConn.Close();//Closes database connection
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                    //returns false
                }
            }
            return Successful;
        }
        //Gets single conversion rate from the database
        public RateClass getSingleConversionRate(CurrencyClass ccFrom, CurrencyClass ccTo)
        {
            RateClass rate = null;
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
            return rate;
        }
        //Gets list of Currencies from the Database
        public ArrayList getCurrencyNames()
        {
            ArrayList tempArray = new ArrayList();
            try
            { 
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
            }
            catch { }
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
        public int InitDatabase()
        {
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
                        bool Created = false;
                        try
                        {
                            Created = CreateDatabase();
                        }
                        catch { }
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
            else//file does not exists, Creare it
            {
                bool Created=false;
                try
                {
                    Created = CreateDatabase();
                }
                catch {}
                if (Created)
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
            return returnCode;
        }
        //Creates Database

        public void CloseDBConn()
        {
            if (MyConn != null)
            {
                MyConn.Close();
            }
        }

        private bool CreateDatabase()
        {
            
            if (!Directory.Exists(ProgramData))
            {
                try
                {
                    Directory.CreateDirectory(ProgramData);
                }
                catch { }
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
                return true;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
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
                //Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                //Exception caught. Failed to connect to database
                return false;
            }
        }

        //Checks to see if provided entry already exists in the Database
        private bool checkForDuplicate(RateClass rate)
        {
            try
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
            catch (Exception ex)
            {
                return false;
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
