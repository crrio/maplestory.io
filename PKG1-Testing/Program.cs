using System;
using System.Diagnostics;
using System.Threading;
using PKG1;
using WZData;
using WZData.MapleStory.Items;

namespace PKG1_Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();
            PackageCollection collection = new PackageCollection("/home/andy/Documents/MapleStory/Library/maplestory/appdata/Base.wz");
            watch.Stop();
            Console.WriteLine($"PKG1 took {watch.ElapsedMilliseconds}ms");
            Console.ReadLine();

            // Equip res = DataFactory.Cache<Equip>(() => Equip.Parse(collection.Resolve("String/Eqp/Eqp/Weapon/1212000")));
        }
    }
}
