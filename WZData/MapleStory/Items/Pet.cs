using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WZData.MapleStory.Items
{
    public class Pet : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Pet";
        public const bool IMGId = true;
        public const string StringPath = "Pet.img";

        public Dictionary<string, IEnumerable<FrameBook>> frameBooks;

        public Pet(int id) : base(id) { }

        public static Pet Parse(int id, WZDirectory itemWzFile, WZObject petEntry, WZObject stringWz, bool showEffects = true)
        {
            Pet p = new Pet(id);
            if (showEffects)
            {
                p.frameBooks = new Dictionary<string, IEnumerable<FrameBook>>();

                foreach (WZObject obj in petEntry)
                {
                    if (obj.Name == "info") continue;
                    p.frameBooks.Add(obj.Name, FrameBook.Parse(itemWzFile, petEntry, obj));
                }
            }

            p.Description = new ItemDescription(
                id,
                stringWz["name"].ValueOrDefault<string>(""),
                stringWz["desc"].ValueOrDefault<string>(""),
                WZFile,
                FolderPath
            );

            p.MetaInfo = ItemInfo.Parse(itemWzFile, petEntry["info"]);

            return p ?? null;
        }

        public static Pet Search(WZFile itemWz, WZFile stringWz, int petId)
        {
            foreach (WZObject petData in stringWz.ResolvePath(StringPath))
            {
                int id = -1;
                if (!int.TryParse(petData.Name, out id) && id != petId)
                    continue;

                WZObject petEntry = null;
                try
                {
                    petEntry = itemWz.ResolvePath(Path.Combine(FolderPath, $"{id}.img"));
                    return Parse(id, itemWz.MainDirectory, petEntry, petData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't resolve pet {id}");
                    continue;
                }
            }

            return null;
        }

        public static IEnumerable<Pet> Parse(WZFile itemWz, WZFile stringWz)
        {
            foreach (WZObject petId in stringWz.ResolvePath(StringPath))
            {
                Console.WriteLine($"Processing {petId.Name}");
                int id = -1;
                if (!int.TryParse(petId.Name, out id))
                    continue;

                WZObject petEntry = null;
                try
                {
                    petEntry = itemWz.ResolvePath(Path.Combine(FolderPath, $"{id}.img"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't resolve pet {id}");
                    continue;
                }

                yield return Parse(id, itemWz.MainDirectory, petEntry, petId);
            }
        }

        public static IEnumerable<Tuple<int, MemoizedThrowFunc<MapleItem>>> GetLookup(WZDirectory itemWz, WZDirectory stringWz)
        {
            foreach (WZObject petId in stringWz.ResolvePath(StringPath))
            {
                int id = -1;
                if (!int.TryParse(petId.Name, out id))
                    continue;

                WZObject petEntry = null;
                try
                {
                    petEntry = itemWz.ResolvePath(Path.Combine(FolderPath, $"{id}.img"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Couldn't resolve pet {id}");
                    continue;
                }

                yield return new Tuple<int, MemoizedThrowFunc<MapleItem>>(id, CreateLookup(id, itemWz, petEntry, petId).Memoize());
            }
            
        }

        private static Func<MapleItem> CreateLookup(int id, WZDirectory itemWz, WZObject petEntry, WZObject petId)
            => ()
            => Parse(id, itemWz, petEntry, petId, true);
    }
}
