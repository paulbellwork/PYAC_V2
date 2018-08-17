using System;
using System.Text;
using System.IO;

namespace Utl
{
    public static class Writer
    {
        public static void writeAll(String text)
        {
            write(text);
            writeOnFile(text);
        }
        public static void write(String text)
        {
            Console.WriteLine(text);
        }

        public static void writeOnFile(String text)
        {
            try
            {
                String logFile = System.Configuration.ConfigurationManager.AppSettings["logFile"];

                StreamWriter sw = new StreamWriter(logFile, true);

                sw.WriteLine(System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString() + "   :" + text);
                sw.Flush();
                sw.Close();
            }
            catch
            { }

        }
    }
}
