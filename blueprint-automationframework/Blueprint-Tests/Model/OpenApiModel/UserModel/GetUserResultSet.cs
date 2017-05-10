using System;
using System.Collections.Generic;
using Model.Impl;
using Newtonsoft.Json;

namespace Model.OpenApiModel.UserModel
{
    public class GetUserResultSet
    {
        public List<GetUserResult> Users { get; set; }
    }

    // Dev code can be found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/User.cs
    public class GetUserResult : UserDataModel
    {
        private string _password;

        #region Serialized properties

        public override bool ShouldSerializeGroups()
        {
            return Groups != null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public new string Password  // NOTE: Password should NEVER be returned by OpenAPI.
        {
            get
            {
                if (_password != null)
                {
                    throw new JsonException("GetUser should NEVER return a Password!");
                }
                return _password;
            }
            set { _password = value; }
        }

        #endregion Serialized properties
    }
}
