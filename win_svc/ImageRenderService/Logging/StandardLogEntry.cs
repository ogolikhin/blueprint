using System;
using ImageRenderService.Collections;

namespace ImageRenderService.Logging
{
    public class StandardLogEntry : LogEntry
    {
        /// <summary>
        /// The level of the log entry.
        /// </summary>
        public Level Level { get; set; }

        /// <summary>
        /// The message of the log entry.
        /// </summary>
        public object Entry { get; set; }

        /// <summary>
        /// The formatted message of the log entry.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// The arguments to the formatted message of the log entry.
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// The exception details of the log entry.
        /// </summary>
        public Exception Exception { get; set; }

        public override string GetContent()
        {
            string text;

            if (string.IsNullOrWhiteSpace(Format))
            {
                text = Entry != null ? Entry.ToString() : string.Empty;
            }
            else
            {
                text = string.Format(Format, Arguments);
            }

            if (Exception != null)
            {
                text = string.Format("{0}\n{1}", text, Exception);
            }

            return text;
        }

        public override Priority GetPriority()
        {
            return Level == Level.Fatal ? Priority.NORMAL : Priority.LOW;
        }
    }
}