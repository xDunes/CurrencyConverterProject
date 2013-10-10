using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.OleDb;
using System.IO;

namespace CurrencyConverter
{
    class DatabaseClass
    {
        private OleDbConnection MyConn; 

        public void saveRate(RateClass rate)
        {
        }
        public RateClass getSingleConversionRate(CurrencyClass ccFrom, CurrencyClass ccTo)
        {
            RateClass rate = null;
            return rate;
        }
        public ArrayList getCurrencyNames()
        {
            ArrayList tempArray = new ArrayList();
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
        private int CheckForDatabase(string path)
        {
            bool ConnStatus = false; //true if connected, false if connection failed
            int returnCode = 0;
            String ConnString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";"; ;
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
                        bool Created = CreateDatabase(path);
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
                bool Created = CreateDatabase(path);
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
        private bool CreateDatabase(string path)
        {
            //Code to create Database
            return true;
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
