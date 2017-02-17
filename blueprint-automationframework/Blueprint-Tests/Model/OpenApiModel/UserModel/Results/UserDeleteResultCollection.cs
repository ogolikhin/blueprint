using System.Collections.Generic;
using Model.OpenApiModel.UserModel.Enums;

namespace Model.OpenApiModel.UserModel.Results
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/Result/Users/UserDeleteResults.cs
    /// <summary>
    /// Describes response returned from DeleteUser call.
    /// </summary>
    public class UserDeleteResultCollection : List<UserDeleteResult>
    {
        public ResultStatusEnum Status { get; set; }
    }
}
