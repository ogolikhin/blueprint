using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
