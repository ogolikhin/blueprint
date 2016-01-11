/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
using System;
using System.Configuration;
using System.IO;

namespace ServiceLibrary.LocalLog
{
    public class LocalFileLog : ILocalLog
    {
        private FileInfo _file;
        private readonly object lockObject = new object();

        public LocalFileLog()
        {
            string fileName = ConfigurationManager.AppSettings["LocalLogFile"] != null
                ? ConfigurationManager.AppSettings["LocalLogFile"]
                : @"C:\Log\BlueprintSys.log";
            _file = new FileInfo(fileName);
        }

        public void LogError(string message)
        {
            WriteMessage("Error", message);
        }

        public void LogInformation(string message)
        {
            WriteMessage("Information", message);
        }

        public void LogWarning(string message)
        {
            WriteMessage("Warning", message);
        }

        private void WriteMessage(string level, string message)
        {
            try
            {
                lock (this.lockObject)
                {
                    using (var writer = new StreamWriter(_file.Open(FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        writer.WriteLine($"[{level}] [{DateTime.Now}] {message}");
                        writer.Flush();
                    }
                }
            }
            catch (Exception)
            {
                // Do Nothing
            }
        }

    }
}
