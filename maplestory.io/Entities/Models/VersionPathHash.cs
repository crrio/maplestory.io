namespace maplestory.io.Entities.Models
{
    public class VersionPathHash
    {
        public long Id { get; set; }
        public long MapleVersionId { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public long? ResolvesTo { get; set; }
        public string PackageName { get; set; }
        public string ImgName { get; set; }

        public MapleVersion MapleVersion { get; set; }
    }
}
