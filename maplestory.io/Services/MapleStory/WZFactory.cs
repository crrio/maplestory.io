using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WZData;
using System.Threading;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class WZFactory : IWZFactory, IDisposable
    {
        private static Dictionary<WZLanguage, Dictionary<WZ, Tuple<string, List<WZFile>>>> _files;
        private static ILogger _logger;
        //private static Thread wzWatcher;

        public static void Load(ILogger<WZFactory> logger, string enWzPath, string krWzPath = null, string jpWzPath = null)
        {
            _logger = logger;
            _files = new Dictionary<WZLanguage, Dictionary<WZ, Tuple<string, List<WZFile>>>>();

            Tuple<WZLanguage, string>[] paths = new[]
            {
                new Tuple<WZLanguage, string>(WZLanguage.English, enWzPath),
                new Tuple<WZLanguage, string>(WZLanguage.Korean, krWzPath),
                new Tuple<WZLanguage, string>(WZLanguage.Japanese, jpWzPath)
            };

            foreach (Tuple<WZLanguage, string> wzEntry in paths.Where(c => c.Item2 != null))
            {
                Dictionary<WZ, Tuple<string, List<WZFile>>> files = new Dictionary<WZ, Tuple<string, List<WZFile>>>();
                string wzPath = wzEntry.Item2;
                _logger?.LogInformation("Caching WZ Files");
                string[] fileNames = Directory.GetFiles(wzPath, "*.wz");

                files = fileNames
                    .Where(c => Path.GetFileNameWithoutExtension(c) != "Data")
                    .Select(c => new Tuple<WZ, string, IEnumerable<WZFile>>((WZ)Enum.Parse(typeof(WZ), Path.GetFileNameWithoutExtension(c), true), c, Enumerable.Range(0, 2).Select(b => new WZFile(c, WZVariant.GMS, false))))
                    .ToDictionary(file => file.Item1, file => new Tuple<string, List<WZFile>>(file.Item2, file.Item3.ToList()));

                _logger?.LogInformation($"Found {files.Count} WZFiles");
                _files.Add(wzEntry.Item1, files);
            }

            //wzWatcher = new Thread(watchWz);
            //wzWatcher.Start();
        }

        //private static void watchWz()
        //{
        //    while (true)
        //    {
        //        foreach(KeyValuePair<WZ, Tuple<string, List<WZFile>>> kvp in _files)
        //        {
        //            List<WZFile> old = kvp.Value.Item2;

        //        }
        //        Thread.Sleep(1);
        //    }
        //}

        public WZFile GetWZFile(WZ file, WZLanguage language = WZLanguage.English)
            => _files.ContainsKey(language) && _files[language].ContainsKey(file) ? _files[language][file].Item2.First() : null;

        public Func<Func<WZFile, MapleItem>, MapleItem> AsyncGetWZFile(WZ file, WZLanguage language = WZLanguage.English)
        {
            return (a) =>
            {
                WZFile wz = _files[language][file].Item2.FirstOrDefault(c => !c.InUse);
                if (wz == null)
                {
                    _logger?.LogInformation($"Provisioning new {file}");
                    wz = new WZFile(_files[language][file].Item1, WZVariant.GMS, false);
                    wz.InUse = true;
                    _files[language][file].Item2.Add(wz);
                } else
                    wz.InUse = true;

                wz.LastUse = DateTime.Now;

                try
                {
                    MapleItem result = a(wz);
                    return result;
                } finally
                {
                    wz.InUse = false;
                }
                return null;
            };
        }

        public void Dispose()
        {
            foreach (var kvp in _files)
                foreach(var languageKvp in kvp.Value)
                    languageKvp.Value.Item2.ForEach((wz) => wz.Dispose());
        }
    }
}
