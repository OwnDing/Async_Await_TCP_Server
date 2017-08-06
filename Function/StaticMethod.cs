using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blank_TCP_Server.Servers.Function
{
    public static class StaticMethod
    {
        /// <summary>
        /// hex to byte[]
        /// </summary>
        /// <param name="hex">hex</param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Contains("-"))
            {
                hex = hex.Replace("-", "");
            }
            else
            {
                hex = hex.Replace(" ", "");
            }           
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        /// <summary>
        /// string to hex
        /// </summary>
        /// <param name="hexstring">string</param>
        /// <returns></returns>
        public static string StringToHex(string hexstring)
        {
            var sb = new StringBuilder();
            foreach (char t in hexstring)
                sb.Append(Convert.ToInt32(t).ToString("x") + " ");
            return sb.ToString();
        }
        /// <summary>
        /// covert hex to ascii string
        /// </summary>
        /// <param name="hex">hex</param>
        /// <returns></returns>
        public static string HexToString(string hex)
        {
            byte[] data = StringToByteArray(hex);
            string result = Encoding.ASCII.GetString(data);
            return result;
        }
    }
}
