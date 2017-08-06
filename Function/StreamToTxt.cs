using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blank_TCP_Server.Function
{
    public class StreamToTxt
    {
        public string ErrPath_Name
        {
            get
            {
                string appPath = Path.GetDirectoryName(Application.ExecutablePath);
                string folder =appPath+ "\\";
                folder = System.IO.Path.Combine(folder, "Log");
                System.IO.Directory.CreateDirectory(folder);
                return folder;
            }
        }
        public void WriteInfo(string str)
        {
            string path = null;
            path = ErrPath_Name + "\\log.txt";

            FileStream fs = new FileStream(path, FileMode.Append);

            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(str);
            byte[] newline = System.Text.Encoding.ASCII.GetBytes(Environment.NewLine);
            //开始写入
            fs.Write(data, 0, data.Length);
            fs.Write(newline, 0, newline.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }
    }
}
