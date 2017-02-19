using System.Collections.Generic;
using Model.OpenApiModel.UserModel.Enums;

namespace Model.OpenApiModel.UserModel.Results
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/Result/Users/UserDeleteResults.cs
    /// <summary>
    /// Describes response returned from various user calls.
    /// </summary>
    public class UserCallResultCollection : List<UserCallResult>
    {
        public ResultStatusEnum Status { get; set; }
    }
}
