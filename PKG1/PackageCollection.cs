using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PKG1
{
    public class PackageCollection
    {
        public static Action<string> Logging = (s) => { };
        public Package BasePackage;
        public string Folder;
        public string BaseFilePath;
        public Dictionary<string, Package> Packages;
        public Region WZRegion;
        public ConcurrentDictionary<string, object> VersionCache;

        public PackageCollection()
        {
            VersionCache = new ConcurrentDictionary<string, object>();
            Packages = new Dictionary<string, Package>();
        }

        public PackageCollection(string baseFilePath, ushort? versionId = null, Region region = Region.GMS) {
            Stopwatch watchGlobal = Stopwatch.StartNew();
            Logging($"Took {watchGlobal.ElapsedMilliseconds}ms to initialize resolver");
            Folder = Path.GetDirectoryName(baseFilePath);
            VersionCache = new ConcurrentDictionary<string, object>();

            WZRegion = region;
            BaseFilePath = baseFilePath;
            BasePackage = new Package(this, baseFilePath, versionId ?? ushort.MinValue);
            if (!BasePackage.VersionMatches || versionId == null) {
                using (WZReader reader = BasePackage.GetContentReader()) {
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        VersionGuesser guesser = new VersionGuesser(reader, baseFilePath);
                        watch.Stop();
                        Logging($"Guessed version in {watch.ElapsedMilliseconds}ms");
                        watch.Restart();
                        BasePackage.UpdateVersion(guesser.VersionId);
                        BasePackage.MainDirectory.Encrypted = guesser.IsEncrypted;
                        watch.Stop();
                        Logging($"Done initializing in {watch.ElapsedMilliseconds}ms");
                    }
                    catch (Exception ex)
                    {
                        Logging($"Failed to guess version for {BaseFilePath}");
                        throw;
                    }
                }
            }

            Packages = BasePackage.MainDirectory.Children
                .Where(c => c.Type == PropertyType.Directory || c.Type == PropertyType.Image || c.Type == PropertyType.Lua)
                .Select(c =>
                {
                    Package res = null;

                    if ((c.Type == PropertyType.Directory || c.Type == PropertyType.Lua) && c.Size < 50)
                    { // I don't know the exact size off hand, I'm assuming it's less than 50 bytes.
                        if (File.Exists(Path.Combine(Folder, $"{c.NameWithoutExtension}.wz")))
                            res = new Package(this, Path.Combine(Folder, $"{c.NameWithoutExtension}.wz"), BasePackage.VersionId);
                        else if (File.Exists(Path.Combine(Folder, $"{c.NameWithoutExtension}.rebuilt.wz")))
                            res = new Package(this, Path.Combine(Folder, $"{c.NameWithoutExtension}.rebuilt.wz"), BasePackage.VersionId);
                        else res = new Package(this) // Create a "ghost" package where the MainDirectory is just the Img
                        {
                            MainDirectory = c
                        };
                    }
                    else res = new Package(this) // Create a "ghost" package where the MainDirectory is just the Img
                    {
                        MainDirectory = c
                    };

                    res.MainDirectory.Encrypted = BasePackage.MainDirectory.Encrypted;

                    return res;
                })
                .ToDictionary(c => c.FileName ?? c.MainDirectory.NameWithoutExtension, c => c);

            if (!Packages.ContainsKey("Map001"))
            {
                string map001Path = Path.Combine(Folder, "Map001.wz");
                string map001RebuiltPath = Path.Combine(Folder, "Map001.rebuilt.wz");
                if (File.Exists(map001Path))
                {
                    Package res = new Package(this, map001Path, BasePackage.VersionId);
                    res.MainDirectory.Encrypted = BasePackage.MainDirectory.Encrypted;
                    Packages.Add("Map001", res);
                }
                else if (File.Exists(map001RebuiltPath))
                {
                    Package res = new Package(this, map001RebuiltPath, BasePackage.VersionId);
                    res.MainDirectory.Encrypted = BasePackage.MainDirectory.Encrypted;
                    Packages.Add("Map001", res);
                }
            }

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

        public void Dispose() {
            foreach (Package p in Packages.Values)
                p.Dispose();
        }
    }
}
