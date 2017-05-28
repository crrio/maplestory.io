﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ;
using System.IO;
using Microsoft.Extensions.Logging;

namespace maplestory.io.Services.MapleStory
{
    public class WZFactory : IWZFactory
    {
        private readonly Dictionary<WZ, WZFile> _files;
        private readonly ILogger _logger;

        public WZFactory(ILogger<WZFactory> logger)
        {
            _logger = logger;
            _files = new Dictionary<WZ, WZFile>();

            _logger?.LogInformation("Caching WZ Files");
            /// TODO: Move to settings
            string maplePath = @"C:\Nexon\Library\maplestory\appdata";
            string[] fileNames = Directory.GetFiles(maplePath, "*.wz");
            IEnumerable<Tuple<WZ,WZFile>> WZFiles = fileNames
                .Where(c => Path.GetFileNameWithoutExtension(c) != "Data")
                .Select(c => new Tuple<WZ, WZFile>((WZ)Enum.Parse(typeof(WZ), Path.GetFileNameWithoutExtension(c), true), new WZFile(c, WZVariant.GMS, false)));
            foreach (Tuple<WZ, WZFile> file in WZFiles) _files.Add(file.Item1, file.Item2);
            _logger?.LogInformation($"Found {_files.Count} WZFiles");
        }

        public WZFile GetWZFile(WZ file) => _files[file];
    }
}