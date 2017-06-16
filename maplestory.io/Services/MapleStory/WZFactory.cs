using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WZData;

namespace maplestory.io.Services.MapleStory
{
    public class WZFactory : IWZFactory, IDisposable
    {
        private readonly Dictionary<WZ, Tuple<string, List<WZFile>>> _files;
        private readonly ILogger _logger;

        public WZFactory(ILogger<WZFactory> logger, IOptions<WZOptions> options) : this(logger, options.Value.WZPath) { }

        public WZFactory(ILogger<WZFactory> logger, string wzPath)
        {
            _logger = logger;
            _files = new Dictionary<WZ, Tuple<string, List<WZFile>>>();

            _logger?.LogInformation("Caching WZ Files");
            string[] fileNames = Directory.GetFiles(wzPath, "*.wz");
            IEnumerable<Tuple<WZ, string, IEnumerable<WZFile>>> WZFiles = fileNames
                .Where(c => Path.GetFileNameWithoutExtension(c) != "Data")
                .Select(c => new Tuple<WZ, string, IEnumerable<WZFile>>((WZ)Enum.Parse(typeof(WZ), Path.GetFileNameWithoutExtension(c), true), c, Enumerable.Range(0, 10).Select(b => new WZFile(c, WZVariant.GMS, false))));
            foreach (Tuple<WZ, string, IEnumerable<WZFile>> file in WZFiles) _files.Add(file.Item1, new Tuple<string, List<WZFile>>(file.Item2, file.Item3.ToList()));
            _logger?.LogInformation($"Found {_files.Count} WZFiles");
        }

        public WZFactory(string wzPath)
        {
            _files = new Dictionary<WZ, Tuple<string, List<WZFile>>>();

            string maplePath = wzPath;
            string[] fileNames = Directory.GetFiles(maplePath, "*.wz");
            IEnumerable<Tuple<WZ, string, WZFile>> WZFiles = fileNames
                .Where(c => Path.GetFileNameWithoutExtension(c) != "Data")
                .Select(c => new Tuple<WZ, string, WZFile>((WZ)Enum.Parse(typeof(WZ), Path.GetFileNameWithoutExtension(c), true), c, new WZFile(c, WZVariant.GMS, false)));
            foreach (Tuple<WZ, string, WZFile> file in WZFiles) _files.Add(file.Item1, new Tuple<string, List<WZFile>>(file.Item2, new List<WZFile>() { file.Item3 }));
        }

        public WZFile GetWZFile(WZ file)
            => _files[file].Item2.First();

        public Func<Func<WZFile, MapleItem>, MapleItem> AsyncGetWZFile(WZ file)
        {
            return (a) =>
            {
                WZFile wz = _files[file].Item2.FirstOrDefault(c => !c.InUse);
                if (wz == null)
                {
                    _logger?.LogInformation($"Provisioning new {file}");
                    wz = new WZFile(_files[file].Item1, WZVariant.GMS, false);
                    wz.InUse = true;
                    _files[file].Item2.Add(wz);
                } else
                    wz.InUse = true;

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
            foreach (KeyValuePair<WZ, Tuple<string, List<WZFile>>> kvp in _files)
                kvp.Value.Item2.ForEach((wz) => wz.Dispose());
        }
    }
}
