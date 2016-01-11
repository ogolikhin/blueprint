using System;
using System.Configuration;
using System.IO;
using ServiceLibrary.Helpers;

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
            LogError(I18NHelper.FormatInvariant(format, args));
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
            LogInformation(I18NHelper.FormatInvariant(format, args));
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
            LogWarning(I18NHelper.FormatInvariant(format, args));
        }

        private string FormatMessage(string level, string message)
        {
            return I18NHelper.FormatInvariant("[{0}] [{1}] {2}", level, DateTime.Now, message);
        }

    }
}
