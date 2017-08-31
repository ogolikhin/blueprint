namespace ServiceLibrary.LocalLog
{
    public class WorkflowLocalLog : LocalFileLog
    {
        public WorkflowLocalLog(string logFileName = null) : base(logFileName)
        {
            
        }

        public override void LogError(string message)
        {
            try { base.LogError(message);}
            catch { }
        }

        public override void LogErrorFormat(string format, params object[] args)
        {
            try { base.LogErrorFormat(format, args);}
            catch { }
        }

        public override void LogInformation(string message)
        {
            try { base.LogInformation(message);}
            catch { }
        }

        public override void LogInformationFormat(string format, params object[] args)
        {
            try { base.LogInformationFormat(format, args);}
            catch { }
        }

        public override void LogWarning(string message)
        {
            try { base.LogWarning(message);}
            catch { }
        }

        public override void LogWarningFormat(string format, params object[] args)
        {
            try { base.LogWarningFormat(format, args);}
            catch {}
        }
    }
}
