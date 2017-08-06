using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class Message
    {
        public string data{get;set;}
        public string ip { get; set; }
        public DateTime getDate { get; set; }

        public Message(){
            getDate = DateTime.Now;
        }

        public override string ToString()
        {
            return "ReceivedData:"+data+";From:"+ip+";Date:"+getDate.ToString();
        }
    }
}
