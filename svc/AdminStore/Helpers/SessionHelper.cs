using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class SessionHelper
    {
        public static int GetUserIdFromSession(HttpRequestMessage httpRequestMessage)
        {
            var session = httpRequestMessage.Properties[ServiceConstants.SessionProperty] as Session;
            if (session == null)
            {
                throw new BadRequestException(ErrorMessages.SessionIsEmpty);
            }
            return session.UserId;
        }
    }
}