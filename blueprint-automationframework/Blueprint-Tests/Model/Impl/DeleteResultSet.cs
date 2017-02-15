﻿using System.Collections.Generic;
using System.Net;

namespace Model.Impl
{
    /// <summary>
    /// Describes responce returned from DeleteUser call
    /// </summary>
    public class DeleteResultSet
    {
        public List<DeleteUserResult> Results { get; set; }
        public HttpStatusCode ReturnCode { get; set; }
    }

    public class DeleteUserResult
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public int ReturnCode { get; set; }
    }
}
