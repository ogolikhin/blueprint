namespace Model.OpenApiModel.UserModel.Enums
{
    // Taken from:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/Enums/ResultStatusEnum.cs
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]    // This is what Dev has.
    public enum ResultStatusEnum
    {
        Ok = 200,
        Success = 201,
        PartialSuccess = 207,
        Failed = 409
    }
}
