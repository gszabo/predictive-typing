using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolingPredType
{
    //class Logger
    //{
    //    static Logger()
    //    {

    //    }

    //    private readonly string logFilePath;

    //    private Logger(string logDir)
    //    {
    //        string logFileName = "log-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
    //        logFilePath = Path.Combine(logDir, logFileName);

    //        Directory.CreateDirectory(logDir);
    //        if (!File.Exists(logFilePath))
    //            File.WriteAllText(logFilePath, "");
    //    }

    //    private static readonly Dictionary<string, Logger> instances = new Dictionary<string, Logger>(); 

    //    private static readonly object syncRoot = new object();

    //    public void Log(string msg)
    //    {
    //        Console.WriteLine(msg);
    //        File.AppendAllText(logFilePath, msg + "\n");
    //    }

    //    public static Logger Create(string logDir)
    //    {
    //        lock (syncRoot)
    //        {
    //            Logger result;
    //            if (instances.TryGetValue(logDir, out result))
    //                return result;

    //            result = new Logger(logDir);
    //            instances.Add(logDir, result);
    //            return result;
    //        }            
    //    }
    //}
}
