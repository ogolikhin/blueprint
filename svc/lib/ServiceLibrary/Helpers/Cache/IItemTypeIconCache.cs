using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Helpers.Cache
{
    public interface IItemTypeIconCache
    {
        void AddOrReplace(IconType predefined, int? itemTypeId, string hexColor, byte[] data);

        byte[] GetValue(IconType predefined, int? itemTypeId, string hexColor);

        void Remove(IconType predefined, int? itemTypeId, string hexColor);
    }
}
