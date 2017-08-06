using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blank_TCP_Server.Function;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class MessageQueue : TaskQueue<Message>
    {
        public MessageQueue(int workerCount) : base(workerCount)
        {

        }

        protected override void Consume()
        {
            while (true)
            {
                Message task;
                lock (locker)
                {
                    while (taskQ.Count == 0) Monitor.Wait(locker);
                    task = taskQ.Dequeue();
                }
                if (task == null) return;         // This signals our exit
                //var s = task.MessageToStringArray();
                Console.WriteLine(task);
                sql.fillTable(task);
                Console.WriteLine();
            }
        }
    }
}
