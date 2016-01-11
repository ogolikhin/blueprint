using System;
using System.Configuration;
using System.Globalization;
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

        public void LogErrorFormat(string format, params object[] args)
        {
            LogError(string.Format(CultureInfo.InvariantCulture, format, args));
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

        public void LogInformationFormat(string format, params object[] args)
        {
            LogInformation(string.Format(CultureInfo.InvariantCulture, format, args));
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

        public void LogWarningFormat(string format, params object[] args)
        {
            LogWarning(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        private string FormatMessage(string level, string message)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}] [{1}] {2}", level, DateTime.Now, message);
        }

    }
}
