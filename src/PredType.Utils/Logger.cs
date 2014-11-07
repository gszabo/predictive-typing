using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredType.Utils
{
    public class Logger
    {
        private readonly string logFilePath;

        public Logger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        /// <summary>
        /// Writes the string parameter to the log with a timestamp and a newline. 
        /// It writes the message to the console and to the log file.
        /// </summary>
        public void Log(string message)
        {
            string msg = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ": " + message + Environment.NewLine;

            Console.Write(msg);
            File.AppendAllText(logFilePath, msg);
        }
    }
}
