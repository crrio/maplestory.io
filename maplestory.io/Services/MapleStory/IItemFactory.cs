﻿using System.Collections.Generic;
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
        IEnumerable<string> GetItemCategories();
        IEnumerable<ItemNameInfo> GetItems(uint startPosition = 0, uint? count = null, string overallCategoryFilter = null, string categoryFilter = null, string subCategoryFilter = null, int? jobFilter = null, bool? cashFilter = null, int? minLevelFilter = null, int? maxLevelFilter = null, int? genderFilter = null);
    }
}
