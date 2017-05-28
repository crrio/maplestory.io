using System.Collections.Generic;
using WZData;
using WZData.MapleStory.Items;

namespace maplestory.io.Services.MapleStory
{
    public interface IItemFactory
    {
        MapleItem search(int id);
        IEnumerable<ItemName> GetItems();
    }
}
