using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.OleDb;
using ADOX;
using System.IO;


namespace CurrencyConverter
{
    class DatabaseClass
    {

        private OleDbConnection MyConn;
        static private string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        
        private string path = ProgramData + "\\CurrencyConverter\\CurrencyDB.mdb";//add database path later
        public bool saveRate(RateClass rate)
        {
            bool Successful = false;
            CurrencyClass ccFrom = rate.getFrom();
            CurrencyClass ccTo = rate.getTo();
            int Status = InitDatabase();
            switch (Status)
            {
                case 0: case 1: case 2:
                    {
                        //Sucess
                        try
                        {
                            string insert = "INSERT into CurrencyConverter (CurFromLong, CurFromShort, CurToLong, CurToShort, Rate, DateTime) VALUES (@CurFromLong, @CurFromShort, @CurToLong, @CurToShort, @Rate, @DateTime)";
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
                        catch
                        {
                            //returns false
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
                            tempArray.Add(reader["CurFromLong"].ToString());
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
            bool ConnStatus = false; //true if connected, false if connection failed
            int returnCode = 0;
            String ConnString = @"Provider=Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";"; 
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
                    catch
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
                bool Created = CreateDatabase();
                if (Created == true)
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
        private bool CreateDatabase()
        {
            
            if (!Directory.Exists(ProgramData + "\\CurrencyConverter"))
            {
                Directory.CreateDirectory(ProgramData + "\\CurrencyConverter");
            }
            //Code to create Database
            Catalog cat = new Catalog();
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
                string CreateString = "Provider=Microsoft.Jet.OLEDB.4.0;DataSource=" + path + ";Jet OLEDB:Engine Type=5";
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
            catch
            {

                return false;
            }
        }
        //Attempts to open database connection. Returns true if successful. 
        private bool OpenDatabaseConn(string ConnString)
        {
            try
            {
                MyConn = new OleDbConnection(ConnString);
                return true;
            }
            catch
            {
                //Exception caught. Failed to connect to database
                return false;
            }
        }
    }
}
