// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************
using System;
using System.Configuration;
using System.IO;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.LocalLog
{
    public class LocalFileLog : ILocalLog
    {
        private FileInfo _file;
        private readonly object lockObject = new object();

        public LocalFileLog() : this(ConfigurationManager.AppSettings["LocalLogFile"])
        {
        }

        public LocalFileLog(string log)
        {
            string fileName = log ?? @"C:\Log\BlueprintSys.log";
            _file = new FileInfo(fileName);
        }

        public bool IsTest { get; set; }    

        public void LogError(string message)
        {
            WriteMessage("Error", message);
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            LogError(I18NHelper.FormatInvariant(format, args));
        }

        public void LogInformation(string message)
        {
            WriteMessage("Information", message);
        }

        public void LogInformationFormat(string format, params object[] args)
        {
            LogInformation(I18NHelper.FormatInvariant(format, args));
        }

        public void LogWarning(string message)
        {
            WriteMessage("Warning", message);
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            LogWarning(I18NHelper.FormatInvariant(format, args));
        }

        private void WriteMessage(string level, string message)
        {
            if (IsTest) return;

            try
            {
                lock (this.lockObject)
                {
                    using (
                        var writer = new StreamWriter(_file.Open(FileMode.Append, FileAccess.Write, FileShare.Read))
                        )
                    {
                        writer.WriteLine(I18NHelper.FormatInvariant("[{0}] [{1}] {2}", level, DateTime.Now, message));
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
