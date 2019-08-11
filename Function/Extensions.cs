using Blank_TCP_Server.Servers.AsyncAwaitServer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blank_TCP_Server.Function
{
    public static class Extensions
    {
        public static string[] MessageToStringArray(this Message msg)
        {
            if ( string.IsNullOrEmpty(msg.data)) return new string[0];
            
            string[] results = msg.data.Split(new[] { ' ',';',':' }, StringSplitOptions.RemoveEmptyEntries);
            return results;
        }

        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {

            var tcs = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);

            return await task;
        }
    }
}
