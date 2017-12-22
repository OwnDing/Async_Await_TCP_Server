using Blank_TCP_Server.Servers.AsyncAwaitServer;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Blank_TCP_Server.SQL
{
    public class SqliteData
    {
        SQLiteConnection dbConnection;
        public SqliteData()
        {
            CreateNewDatabase();
        }

        void CreateNewDatabase()
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string databasefile = path + "\\Flapping.sqlite";
            if (!System.IO.File.Exists(databasefile))
            {
                SQLiteConnection.CreateFile("Flapping.sqlite");
                ConnectToDatabase();
                CreateTable();
                return;
            }
            ConnectToDatabase();
        }

        void ConnectToDatabase()
        {
            dbConnection = new SQLiteConnection("Data Source=Flapping.sqlite;Version=3;");
            dbConnection.Open();
        }

        void CreateTable()
        {
            string sql = "create table flappingdata (id INTEGER primary key, dt datetime default current_timestamp,lapping VARCHAR(180),ipaddress VARCHAR(30))";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }
        public void FillTable(Message msg)
        {
            try
            {
                using (var tx = dbConnection.BeginTransaction())
                {
                    string sql = "insert into flappingdata (lapping,ipaddress) values ('" + msg.data + "','" + msg.ip + "')";
                    SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                    command.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            catch (SQLiteException e)
            {
                ConnectToDatabase();
                Console.WriteLine("Error in Sqlite Connection. " + e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Sqlite. "+e.ToString());
            }
            
        }

        public void FillTable(List<Message> list)
        {
            try
            {
                using (var tx = dbConnection.BeginTransaction())
                {
                    foreach (var msg in list)
                    {
                        string sql = "insert into flappingdata (dt,lapping,ipaddress) values ('" + msg.getDate + "','" + msg.data + "','" + msg.ip + "')";
                        SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                        command.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            catch (SQLiteException e)
            {
                ConnectToDatabase();
                Console.WriteLine("Error in Sqlite Connection. " + e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Sqlite. " + e.ToString());
            }
        }

        public void ConShutDown()
        {
            dbConnection.Close();
        }
    }
}
