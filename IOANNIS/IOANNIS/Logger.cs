using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOANNIS
{
    public class Logger
    {
        public static void Write(string line)
        {
            StreamWriter sw = new StreamWriter("log.txt");
            {
                sw.WriteLine( DateTime.Now.ToString() + " " + line, true);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
