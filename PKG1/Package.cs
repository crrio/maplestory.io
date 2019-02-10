using Amazon.S3.Model;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PKG1
{
    public class Package : IDisposable {
        public static Action<string> Logging = (s) => { };
        StreamFactory streamFactory;
        public PackageCollection Collection;
        public string FileName;
        public string FilePath;
        public long FileSize;
        public uint ContentsStartLocation;
        public string FileDescription;
        public ushort VersionId;
        public byte VersionHash;
        public uint VersionKey;
        public bool VersionMatches;
        public WZProperty MainDirectory;

        public Package(PackageCollection parent)
        {
            Collection = parent;
        }

        public Package(PackageCollection parent, string fileLocation, ushort version = ushort.MinValue, bool? isImg = null) {
            Collection = parent;
            FilePath = fileLocation;
            FileName = Path.GetFileNameWithoutExtension(FilePath).Replace(".rebuilt", "");
            streamFactory = new StreamFactory(() => File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            isImg = isImg ?? fileLocation.EndsWith(".img");

            using(WZReader file = GetRawReader()) {
                if (!isImg.Value)
                {
                    if (!file.ReadString(4).Equals("PKG1", StringComparison.CurrentCultureIgnoreCase)) throw new InvalidOperationException("Can not run on non-PKG1 files.");
                    FileSize = file.ReadInt64();
                    ContentsStartLocation = file.ReadUInt32();
                    FileDescription = file.ReadNULTerminatedString(100);
                    Logging($"Loaded package {this.FileName} ({this.FileSize}, {this.ContentsStartLocation}) with Description: {this.FileDescription}");
                }
                MainDirectory = new WZProperty(FileName, FileName, this, isImg.Value ? PropertyType.Image : PropertyType.Directory, null, (uint)FileSize, -1, (uint)ContentsStartLocation + (uint)(isImg.Value ? 0 : 2));

                file.BaseStream.Seek(ContentsStartLocation, SeekOrigin.Begin);

                if (!isImg.Value)
                {
                    this.VersionHash = (byte)file.ReadUInt16();
                    Logging($"{this.FileName} - Version Hash: {this.VersionHash}");
                    if (!UpdateVersion(version))
                        Logging("Warning: Version is not wrong, expect corrupted / malformed data.");
                }
            }
        }

        public WZProperty Resolve(string v)
            => MainDirectory.Resolve(v);

        public bool UpdateVersion(ushort version) {
            if (VersionMatches = CheckVersionMatch(version, out VersionKey)) VersionId = version;
            string correctText = VersionMatches ? "correct" : "incorrect";
            Logging($"{FileName} - {version} is the {correctText} Version Id ( {VersionHash}, {VersionKey} )");
            return VersionMatches;
        }

        bool CheckVersionMatch(ushort version, out uint versionKey) => this.VersionHash == CalcVersionHash(version, out versionKey);
        byte CalcVersionHash(ushort version, out uint versionKey) {
            uint key = version.ToString().Select(c => (uint)c).Aggregate((uint)0, (result, next) => {
                result <<= 5;
                result += (next + 1);
                return result;
            });
            versionKey = key;
            return BitConverter.GetBytes(key).Aggregate((byte)0xFF, (result, next) => (byte)(result ^ next));
        }

        public WZReader GetRawReader(Encoding encoding = null, WZProperty container = null) => new WZReader(this, container ?? MainDirectory, GetRawStream(), encoding ?? Encoding.ASCII, VersionKey, VersionHash, ContentsStartLocation);
        public WZReader GetContentReader(Encoding encoding = null, WZProperty container = null) => new WZReader(this, container ?? MainDirectory, GetContentStream(container), encoding ?? Encoding.ASCII, VersionKey, VersionHash, ContentsStartLocation);
        public Stream GetRawStream() => streamFactory.GetStream();
        public Stream GetContentStream(WZProperty container = null) {
            Stream str = GetRawStream();
            str.Seek((container ?? MainDirectory).ContainerStartLocation, SeekOrigin.Begin);
            return str;
        }

        public void Dispose() => this.streamFactory.Dispose();
    }
}