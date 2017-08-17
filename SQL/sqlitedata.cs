using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Blank_TCP_Server.Servers.AsyncAwaitServer;

namespace Blank_TCP_Server.SQL
{
    public class sqlitedata
    {
        SQLiteConnection m_dbConnection;
        public sqlitedata()
        {
            createNewDatabase();
        }

        void createNewDatabase()
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string databasefile = path + "\\Flapping.sqlite";
            if (!System.IO.File.Exists(databasefile))
            {
                SQLiteConnection.CreateFile("Flapping.sqlite");
                connectToDatabase();
                createTable();
                return;
            }
            connectToDatabase();
        }

        void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=Flapping.sqlite;Version=3;");
            m_dbConnection.Open();
        }

        void createTable()
        {
            string sql = "create table flappingdata (id INTEGER primary key, dt datetime default current_timestamp,lapping VARCHAR(180),ipaddress VARCHAR(30))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
        public void fillTable(Message msg)
        {
            try
            {
                using (var tx = m_dbConnection.BeginTransaction())
                {
                    string sql = "insert into flappingdata (lapping,ipaddress) values ('" + msg.data + "','" + msg.ip + "')";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            catch (SQLiteException e)
            {
                connectToDatabase();
                Console.WriteLine("Error in Sqlite Connection. " + e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Sqlite. "+e.ToString());
            }
            
        }

        public void fillTable(List<Message> list)
        {
            try
            {
                using (var tx = m_dbConnection.BeginTransaction())
                {
                    foreach (var msg in list)
                    {
                        string sql = "insert into flappingdata (dt,lapping,ipaddress) values ('" + msg.getDate + "','" + msg.data + "','" + msg.ip + "')";
                        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                        command.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            catch (SQLiteException e)
            {
                connectToDatabase();
                Console.WriteLine("Error in Sqlite Connection. " + e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Sqlite. " + e.ToString());
            }
        }

        public void conShutDown()
        {
            m_dbConnection.Close();
        }
    }
}
