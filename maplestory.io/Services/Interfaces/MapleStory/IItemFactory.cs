using maplestory.io.Data;
using maplestory.io.Data.Items;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IItemFactory
    {
        MapleItem Search(int id);
        Image<Rgba32> GetIcon(int id);
        Image<Rgba32> GetIconRaw(int id);
        bool DoesItemExist(int id);
        Dictionary<string, Dictionary<string, Tuple<string, int, int>[]>> GetItemCategories();
        IEnumerable<ItemNameInfo> GetItems(int startPosition = 0, int? count = null, string overallCategoryFilter = null, string categoryFilter = null, string subCategoryFilter = null, int? jobFilter = null, bool? cashFilter = null, int? minLevelFilter = null, int? maxLevelFilter = null, int? genderFilter = null, string searchFor = null);
        Tuple<ItemNameInfo, IconInfo, EquipInfo>[] BulkItemInfo(int[] itemIds);
    }
}
