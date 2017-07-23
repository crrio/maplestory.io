﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PKG1
{
    public class PackageCollection
    {
        public static Action<string> Logging = (s) => { };
        public Package BasePackage;
        public string Folder;
        public string BaseFilePath;
        public Dictionary<string, Package> Packages;
        public PropertyResolvers Resolver;
        public Region WZRegion;
        public PackageCollection(string baseFilePath, ushort versionId = ushort.MinValue, Region region = Region.GMS) {
            Stopwatch watchGlobal = Stopwatch.StartNew();
            this.Resolver = new PropertyResolvers(this);
            Logging($"Took {watchGlobal.ElapsedMilliseconds}ms to initialize resolver");
            Folder = Path.GetDirectoryName(baseFilePath);

            WZRegion = region;
            BaseFilePath = baseFilePath;
            BasePackage = new Package(this, baseFilePath, versionId);
            if (!BasePackage.VersionMatches) {
                using (WZReader reader = BasePackage.GetContentReader()) {
                    Stopwatch watch = Stopwatch.StartNew();
                    VersionGuesser guesser = new VersionGuesser(reader);
                    watch.Stop();
                    Logging($"Guessed version in {watch.ElapsedMilliseconds}ms");
                    watch.Restart();
                    BasePackage.UpdateVersion(guesser.VersionId);
                    BasePackage.Parse();
                    watch.Stop();
                    Logging($"Done initializing in {watch.ElapsedMilliseconds}ms");
                }
            }

            Packages = BasePackage.MainDirectory.Children
                .Where(c => c.Value.Type == PropertyType.Directory)
                .AsParallel()
                .Select(c => new Package(this, Path.Combine(Folder, $"{c.Key}.wz"), BasePackage.VersionId))
                .ToDictionary(c => c.FileName, c => c);

            Packages.Add(BasePackage.FileName, BasePackage);

            watchGlobal.Stop();
            Logging($"Took {watchGlobal.ElapsedMilliseconds}ms total");
        }

        public WZProperty Resolve(string path) {
            int slashStart = path.IndexOf('/');
            if (slashStart == -1) slashStart = path.Length;
            string wzName = path.Substring(0, slashStart);

            if (Packages.ContainsKey(wzName)) return Packages[wzName].Resolve(path.Substring(Math.Min(slashStart + 1, path.Length)));

            return null;
        }
    }
}