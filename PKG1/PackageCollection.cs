using System;
using System.Collections.Concurrent;
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
        public ConcurrentDictionary<string, object> VersionCache;

        public PackageCollection(string baseFilePath, ushort versionId = ushort.MinValue, Region region = Region.GMS) {
            Stopwatch watchGlobal = Stopwatch.StartNew();
            this.Resolver = new PropertyResolvers(this);
            Logging($"Took {watchGlobal.ElapsedMilliseconds}ms to initialize resolver");
            Folder = Path.GetDirectoryName(baseFilePath);
            VersionCache = new ConcurrentDictionary<string, object>();

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
            int forwardSlashPosition = path.IndexOf('/');
            int backSlashPosition = path.IndexOf('\\', 0, forwardSlashPosition == -1 ? path.Length : forwardSlashPosition);
            int firstSlash = -1;

            if (forwardSlashPosition == -1) firstSlash = backSlashPosition;
            else if (backSlashPosition == -1) firstSlash = forwardSlashPosition;
            else firstSlash = Math.Min(forwardSlashPosition, backSlashPosition);

            if (firstSlash == -1) firstSlash = path.Length;
            string wzName = path.Substring(0, firstSlash);

            if (Packages.ContainsKey(wzName)) return Packages[wzName].Resolve(path.Substring(Math.Min(firstSlash + 1, path.Length)));

            return null;
        }
    }
}
