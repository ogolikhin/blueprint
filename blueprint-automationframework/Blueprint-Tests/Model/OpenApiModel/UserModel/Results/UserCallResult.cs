using Model.Impl;

namespace Model.OpenApiModel.UserModel.Results
{
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/Result/Users/UserDeleteResults.cs
    public class UserCallResult
    {
        public UserDataModel User { get; set; }
        public string Message { get; set; }
        public int ResultCode { get; set; }
    }
}
