
namespace Model.NovaModel.Reviews
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ViewStateType : byte
    {
        NotViewed = 0,
        Viewed = 1,
        Changed = 2,
        NotRequested = 3
    }
}
