using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Facebook.Tools.EventCreator
{
    public class Logger
    {
        private const string LOG_FILE_NAME = "log.txt";

        public static void LogInfo(string info)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, LOG_FILE_NAME);

            if (!File.Exists(filePath))
            {
                var fs = File.Create(filePath);
                fs.Close();
            }

            using (var fs = File.OpenWrite(filePath))
            {
                fs.Position = fs.Length;
                var writer = new StreamWriter(fs);
                writer.WriteLine("{0}-----------------------------------------", DateTime.Now.ToString());
                writer.WriteLine(info);
                writer.Flush();                
                writer.Close();
            }
        }
    }
}
