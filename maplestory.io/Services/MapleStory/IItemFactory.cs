using System.Collections.Generic;
using ImageSharp;
using PKG1;
using WZData;
using WZData.MapleStory.Items;

namespace maplestory.io.Services.MapleStory
{
    public interface IItemFactory : INeedWZ<IItemFactory>
    {
        MapleItem search(int id);
        Image<Rgba32> GetIcon(int id);
        Image<Rgba32> GetIconRaw(int id);
        bool DoesItemExist(int id);
        IEnumerable<ItemNameInfo> GetItems();
        IEnumerable<string> GetItemCategories();
    }
}
