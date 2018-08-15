using System;
using System.Collections.Generic;
using System.Linq;
using maplestory.io.Data.Characters;
using maplestory.io.Models;
using maplestory.io.Services.Interfaces.MapleStory;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class AvatarFactory : IAvatarFactory
    {
        public AvatarFactory(IWZFactory wzFactory) => this._wzFactory = wzFactory;
        private IWZFactory _wzFactory;

        void LoadCharacter(Character c)
        {
            WZProperty body = null, head = null;
            bool hasChair = false, hasMount = false;
            string chairSitAction = "sit";
            Dictionary<AvatarItemEntry, WZProperty> resolved = ResolveItems(c, ref body, ref head, ref hasChair, ref hasMount, ref chairSitAction);
        }

        private Dictionary<AvatarItemEntry, WZProperty> ResolveItems(Character c, ref WZProperty body, ref WZProperty head, ref bool hasChair, ref bool hasMount, ref string chairSitAction)
        {
            Dictionary<AvatarItemEntry, WZProperty> resolved = new Dictionary<AvatarItemEntry, WZProperty>();
            List<AvatarItemEntry> explorativeSearch = new List<AvatarItemEntry>();
            foreach (AvatarItemEntry item in c.ItemEntries)
            {
                MSPackageCollection wz = _wzFactory.GetWZ(item.Region, item.Version);
                hasMount = hasMount || item.ItemId >= 1902000 && item.ItemId <= 1993000;
                bool isChair = item.ItemId / 10000 == 301;
                if (isChair)
                {
                    hasChair = true;
                    chairSitAction = wz.Resolve($"Item/Install/0301.img/{item.ItemId.ToString("D8")}/info/sitAction")?.ResolveForOrNull<string>() ?? "sit";
                }
                bool isBody = item.ItemId < 10000;
                bool isHead = item.ItemId < 20000;

                if (isBody) body = wz.Resolve("Character").Resolve(item.ItemId.ToString("D8"));
                else if (isHead) head = wz.Resolve("Character").Resolve(item.ItemId.ToString("D8"));
                else
                {
                    int category = item.ItemId / 100;

                    WZProperty node;
                    if (category == 301) resolved.Add(item, wz.Resolve($"Item/Install/0301/{item.ItemId.ToString("D8")}"));
                    else if (category == 501) resolved.Add(item, wz.Resolve($"Item/Cash/0501/{item.ItemId.ToString("D8")}"));
                    else
                    {
                        if (wz.categoryFolders.TryGetValue(category, out string folder))
                            resolved.Add(item, wz.Resolve($"Character/{folder}/{item.ItemId.ToString("D8")}"));
                        else
                            explorativeSearch.Add(item);
                    }
                }
            }

            if (explorativeSearch.Count > 0)
            {
                foreach (var regionGrouping in explorativeSearch.GroupBy(a => a.Region))
                {
                    foreach (var versionGrouping in regionGrouping.GroupBy(a => a.Version))
                    {
                        string[] searchingFor = versionGrouping.Select(b => b.ItemId.ToString("D8")).ToArray();
                        List<WZProperty> found = new List<WZProperty>();
                        MSPackageCollection wz = _wzFactory.GetWZ(regionGrouping.Key, versionGrouping.Key);
                        IEnumerable<WZProperty> allItems = wz.Resolve("Character").Children.SelectMany(b => b.Children);
                        foreach (WZProperty itemNode in allItems)
                        {
                            if (searchingFor.Contains(itemNode.NameWithoutExtension))
                            {
                                resolved.Add(versionGrouping.First(b => b.ItemId.ToString("D8") == itemNode.NameWithoutExtension), itemNode);
                            }
                        }
                    }
                }
            }

            return resolved;
        }

        public Image<Rgba32> Render(Character c)
        {
            throw new NotImplementedException();
        }
    }
}
