using System;
using System.Runtime.Serialization;
using System.Web.Http;
using ServiceLibrary.Helpers;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace AdminStore.Saml
{
    [Serializable]
    public class FederatedAuthenticationException : AuthenticationException
    {
        private readonly FederatedAuthenticationErrorCode _errorCode;
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public FederatedAuthenticationErrorCode ErrorCode
        {
            get { return _errorCode; }
        }

        public FederatedAuthenticationException(FederatedAuthenticationErrorCode errorCode)
        {
            _errorCode = errorCode;
        }

        public FederatedAuthenticationException(string message, FederatedAuthenticationErrorCode errorCode)
            : base(message)
        {
            _errorCode = errorCode;
        }

        public FederatedAuthenticationException(string message, FederatedAuthenticationErrorCode errorCode, Exception inner)
            : base(message, inner)
        {
            _errorCode = errorCode;
        }

        protected FederatedAuthenticationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            //TODO use reflection to get property name
            info.AddValue(ServiceConstants.ErrorCodeName, ErrorCode);
        }

        public HttpError CreateHttpError(int errorCode = -1)
        {
            if (errorCode < 0)
            {
                errorCode = (int)ErrorCode;
            }
            return new HttpError(Message) { { ServiceConstants.ErrorCodeName, errorCode } };
        }

    }
}