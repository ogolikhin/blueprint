using System;
using System.Configuration;
using System.IO;

namespace ServiceLibrary.LocalLog
{
    public class LocalFileLog : ILocalLog
    {
        private FileInfo _file;

        public LocalFileLog()
        {
            string fileName = ConfigurationManager.AppSettings["LocalLogFile"] != null
                ? ConfigurationManager.AppSettings["LocalLogFile"]
                : @"C:\Log\Blueprint.log";
            _file = new FileInfo(fileName);
        }

        public async void LogError(string message)
        {
            try
            {
                using (var writer = new StreamWriter(_file.Open(FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(FormatMessage("Error", message));
                }
            }
            catch (Exception)
            {
                // Do Nothing
            }
        }

        public async void LogInformation(string message)
        {
            try
            {
                using (var writer = new StreamWriter(_file.Open(FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(FormatMessage("Information", message));
                }
            }
            catch (Exception)
            {
                // Do Nothing
            }
        }

        public async void LogWarning(string message)
        {
            try
            {
                using (var writer = new StreamWriter(_file.Open(FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(FormatMessage("Warning", message));
                }
            }
            catch (Exception)
            {
                // Do Nothing
            }
        }

        private string FormatMessage(string level, string message)
        {
            return $"[{level}] [{DateTime.Now}] {message}";
        }

    }
}
