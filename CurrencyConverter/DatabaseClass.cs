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
                }//endtry
                catch {}
            }//endif
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
                }//endtry
                catch
                {
                    //returns false
                }//endcatch
            }//endelse
            return Successful;
        }//Saverate

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
            }//endif
            else
            {
                double rateVal = 0;
                DateTime dateAdded = DateTime.Now; 
                while (reader.Read())
                {
                    rateVal = Convert.ToDouble(reader["Rate"].ToString());
                    dateAdded = Convert.ToDateTime(reader["DateTime"].ToString());
                }//emdwhile
                rate = new RateClass(ccFrom, ccTo, rateVal, dateAdded);
            }//endelse
            return rate;
        }//getSingleConversionRate

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
                    }//endif
                    if (!tempArray.Contains(ccTo))
                    {
                        tempArray.Add(ccTo);
                    }//endif
                }//endwhile
            }//endtry
            catch { }
            return tempArray;
        }//getCurrencyNames

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
            String ConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";"; //Database Connection String
            //Check to see if database file exists
            
            if (File.Exists(path))
            {
                ConnStatus = OpenDatabaseConn(ConnString);
                if (ConnStatus == true)
                {
                    returnCode = 0;
                }//endif
                else //Database is corrupt. Attempt to re-create
                {
                    try
                    {
                        File.Delete(path);
                    }//endtry
                    catch
                    {
                        returnCode = 4;
                    }//endcatch
                    if (returnCode != 4)
                    {
                        //Create new database and attempt to connect
                        bool Created = false;
                        try
                        {
                            Created = CreateDatabase();
                        }//endtry
                        catch { }
                        if (Created == true)
                        {
                            ConnStatus = OpenDatabaseConn(ConnString);
                            if (ConnStatus == true)
                            {
                                returnCode = 2;
                            }//endif
                            else
                            {
                                returnCode = 5;
                            }//endelse
                        }//endif
                        else
                        {
                            returnCode = 3;
                        }//endelse
                    }//endif
                }//endelse
            }//endif
            else//file does not exists, Creare it
            {
                bool Created=false;
                try
                {
                    Created = CreateDatabase();
                }
                catch { }
                if (Created)
                {
                    ConnStatus = OpenDatabaseConn(ConnString);
                    if (ConnStatus == true)
                    {
                        returnCode = 1;
                    }//endif
                    else
                    {
                        returnCode = 4;
                    }//endelse
                }//endif
                else
                {
                    returnCode = 3;
                }//endelse
            }//endelse
            return returnCode;
        }//initDatabase

        //Closes Database Connection
        public void CloseDBConn()
        {
            if (MyConn != null)
            {
                MyConn.Close();
            }
        }//CloseDBConn

        //Creates Database
        private bool CreateDatabase()
        {
            //Checks to see if CurrencyConverter Folder exists in ProgramData. Creates if non-existant
            if (!Directory.Exists(ProgramData))
            {
                try
                {
                    Directory.CreateDirectory(ProgramData);
                }//endtry
                catch { }
            }//endif

            //Code to create Database
            //Creates Database Table
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
                //Creates Database File
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
            }//endtry
            catch
            {
                //Debug.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
                return false;
            }//endcatch
            
        }//InitDatabase

        //Attempts to open database connection. Returns true if successful. 
        private bool OpenDatabaseConn(string ConnString)
        {
            try
            {
                MyConn = new OleDbConnection(ConnString);
                MyConn.Open();
                return true;
            }//endtry
            catch
            {
                //Exception caught. Failed to connect to database
                return false;
            }//endcatch
        }//OpenDatabaseConnection

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
            catch
            {
                return false;
            }
        }//checkForDuplicate
    }//DatabaseClass
}//CurrencyConverter
