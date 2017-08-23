using MailBee;
using ErrorCodes = ServiceLibrary.Helpers.ErrorCodes;

namespace AdminStore.Services.Email
{
    public static class EmailClientExceptionHandler
    {
        public static void HandleConnectException(MailBeeException ex)
        {
            int errorCodeToReturn = ErrorCodes.UnknownIncomingMailServerError;

            if (ex is MailBeeGetRemoteHostNameException)
            {
                errorCodeToReturn = ErrorCodes.IncomingMailServerInvalidHostname;
            }

            throw new EmailException(ex.Message, errorCodeToReturn);
        }

        public static void HandleLoginException(MailBeeException ex)
        {
            int errorCodeToReturn = ErrorCodes.UnknownIncomingMailServerError;

            if (ex is IMailBeeLoginBadCredentialsException)
            {
                errorCodeToReturn = ErrorCodes.IncomingMailServerInvalidCredentials;
            }

            throw new EmailException(ex.Message, errorCodeToReturn);
        }
    }
}
