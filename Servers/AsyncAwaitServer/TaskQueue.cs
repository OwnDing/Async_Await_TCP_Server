using Blank_TCP_Server.SQL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class TaskQueue<T> : IDisposable where T : class
    {
        protected object locker = new object();
        Thread[] workers;
        protected Queue<T> taskQ = new Queue<T>();
        protected sqlitedata sql = new sqlitedata();

        public TaskQueue(int workerCount)
        {
            workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++)
                (workers[i] = new Thread(Consume)).Start();
        }

        public void Dispose()
        {
            // Enqueue one null task per worker to make each exit.
            foreach (Thread worker in workers) EnqueueTask(null);
            foreach (Thread worker in workers) worker.Join();
            sql.conShutDown();
        }

        public virtual void EnqueueTask(T task)
        {
            lock (locker)
            {
                if (taskQ.Count < 100)
                {
                    taskQ.Enqueue(task);
                }
                Monitor.PulseAll(locker);
            }
        }

        protected virtual void Consume()
        {
            while (true)
            {
                T task;
                lock (locker)
                {
                    while (taskQ.Count == 0) Monitor.Wait(locker);
                    task = taskQ.Dequeue();
                }
                if (task == null) return;         // This signals our exit
                Console.WriteLine(task);
                //sql.fillTable(task.ToString());
                Console.WriteLine();
            }
        }
    }
}
