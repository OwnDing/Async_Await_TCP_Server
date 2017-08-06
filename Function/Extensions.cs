using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blank_TCP_Server.Servers.AsyncAwaitServer;

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
    }
}
