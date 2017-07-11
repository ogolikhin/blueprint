namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public class StackTraceProvider : ServiceLocator<IStackTraceProvider>
    {
        private StackTraceProvider()
        {
            // hide
        }
    }
}
