using System.Collections.Generic;
using PKG1;
using WZData;
using WZData.MapleStory.Items;

namespace maplestory.io.Services.MapleStory
{
    public interface IItemFactory : INeedWZ<IItemFactory>
    {
        MapleItem search(int id);
        IEnumerable<ItemNameInfo> GetItems();
        IEnumerable<string> GetItemCategories();
    }
}
