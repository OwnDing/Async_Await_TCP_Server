using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blank_TCP_Server.SQL;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class MessageDictionary
    {
        private  Mutex mut = new Mutex();
        private List<Message> list;
        private sqlitedata sql;

        public MessageDictionary()
        {
            list = new List<Message>();
            sql = new sqlitedata();
        }
        
        public void Add(Message msg)
        {
            mut.WaitOne();
            list.Add(msg);
            try
            {
                if (list.Count > 20)
                {
                    UpdateSql();
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        public void UpdateSql()
        {
            sql.fillTable(list);
            list.Clear();
        }

        public void SqlShutDown()
        {
            sql.conShutDown();
        }
    }
}
