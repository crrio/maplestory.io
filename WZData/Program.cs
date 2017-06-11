using WZData.MapleStory.Items;
using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;

namespace WZData
{
    public class Program
    {
    //    [Stage("potentials")]
    //    static Thread potentials(string WZPath)
    //    {
    //        WZFile itemWz = new WZFile(Path.Combine(WZPath, "Item.wz"), WZVariant.GMS, false);
    //        List<Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>>> potentialCombos = GetPotentials(itemWz).ToList();

    //        potentialData = potentialCombos.Select(c => c.Item1).Where(c => c != null).DistinctBy((p) => p.id);
    //        return null;//UploadManager.Upload("Potentials Upload", potentials, "maplestory", "newPotentials");
    //    }
    //    [Stage("levels")]
    //    static Thread levels(string WZPath)
    //    {
    //        WZFile itemWz = new WZFile(Path.Combine(WZPath, "Item.wz"), WZVariant.GMS, false);
    //        List<Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>>> potentialCombos = GetPotentials(itemWz).ToList();

    //        itemPotentialData = potentialCombos.Select(c => c.Item2).Where(c => c != null).SelectMany(c => c).ToList();
    //        return null;//UploadManager.Upload("Potential Levels Upload", levels, "maplestory", "newPotentialLevels");
    //    }

        //static IEnumerable<Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>>> GetPotentials(WZFile itemLookup)
        //{
        //    foreach (WZObject pot in itemLookup.ResolvePath("/ItemOption.img"))
        //        yield return ItemPotential.Parse(pot);
        //}

        //static IEnumerable<World> GetWorldIcons()
        //{
        //    return Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "assets", "worlds")).Select(fileName =>
        //    {
        //        using (MemoryStream mem = new MemoryStream()) {
        //            string worldName = Path.GetFileNameWithoutExtension(fileName);
        //            Image<Rgba32>.FromFile(fileName).Save(mem, ImageFormat.Png);
        //            byte[] memBytes = mem.ToArray();
        //            return new World()
        //            {
        //                id = worldName.ToLower(),
        //                Icon = Convert.ToBase64String(memBytes)
        //            };
        //        }
        //    });
        //}
    }
}
