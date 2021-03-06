﻿using System;
using System.Threading;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class MessageQueue : TaskQueue<Message>
    {
        MessageDictionary msgDic = new MessageDictionary();

        public MessageQueue(int workerCount) : base(workerCount)
        {

        }

        public void Stop()
        {
            msgDic.SqlShutDown();
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
                msgDic.Add(task);
                Console.WriteLine();
            }
        }
    }
}
