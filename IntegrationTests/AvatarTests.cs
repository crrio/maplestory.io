using ImageSharp;
using maplestory.io;
using maplestory.io.Services.MapleStory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace IntegrationTests
{
    public class AvatarTests
    {
        private static readonly WZFactory wzFactory;
        private static readonly ItemFactory itemFactory;
        private static readonly ZMapFactory zmapFactory;
        private static readonly CharacterFactory characterFactory;

        static AvatarTests()
        {
            wzFactory = new WZFactory(WZPath);
            itemFactory = new ItemFactory(wzFactory, null, null);
            zmapFactory = new ZMapFactory(wzFactory);
            characterFactory = new CharacterFactory(wzFactory, itemFactory, zmapFactory);
        }

        public static string WZPath { get => File.ReadAllText("wzpath.txt"); }

        public static IEnumerable<object[]> GetAvatars()
        {
            return new[]
            {
                new object[]{ File.ReadAllBytes("Resources/inu.png"), 2003, null, 0, false, 2, new[] { new Tuple<int, string>(20544, null), new Tuple<int, string>(38006, null), new Tuple<int, string>(1004757, null), new Tuple<int, string>(1053045, null), new Tuple<int, string>(1073145, null), new Tuple<int, string>(1012055, null) } },
                new object[]{ File.ReadAllBytes("Resources/Phantom.png"), 2003, null, 0, false, 2, new[] { new Tuple<int, string>(1182060, null), new Tuple<int, string>(1672040, null), new Tuple<int, string>(1012438, null), new Tuple<int, string>(1022211, null), new Tuple<int, string>(1032223, null), new Tuple<int, string>(1113073, null), new Tuple<int, string>(1113074, null), new Tuple<int, string>(1113075, null), new Tuple<int, string>(1113072, null), new Tuple<int, string>(1122267, null), new Tuple<int, string>(1122267, null), new Tuple<int, string>(1152108, null), new Tuple<int, string>(1362090, null), new Tuple<int, string>(1352103, null), new Tuple<int, string>(1003800, null), new Tuple<int, string>(1042257, null), new Tuple<int, string>(1062168, null), new Tuple<int, string>(1072743, null), new Tuple<int, string>(1082543, null), new Tuple<int, string>(1102481, null), new Tuple<int, string>(1132174, null), new Tuple<int, string>(1190521, null), new Tuple<int, string>(30330, null), new Tuple<int, string>(20023, null) } },
                new object[]{ File.ReadAllBytes("Resources/tylerbaka.png"), 2003, null, 0, false, 2, new[] { new Tuple<int, string>(20544, null), new Tuple<int, string>(38006, null), new Tuple<int, string>(1004757, null), new Tuple<int, string>(1053045, null), new Tuple<int, string>(1073145, null), new Tuple<int, string>(1012055, null) } }
            };
        }

        [Theory]
        [MemberData(nameof(GetAvatars))]
        public void AvatarGeneration(byte[] expectedResults, int skinId, string animation, int frame, bool showEars, int padding, Tuple<int, string>[] itemEntries)
        {
            Image<Rgba32> characterAvatar = characterFactory.GetCharacter(skinId, animation, frame, showEars, padding, itemEntries);
            byte[] characterAvatarBytes = characterAvatar.ImageToByte();
            bool isAllEqual = characterAvatarBytes.Select((c, i) => expectedResults[i] == c).All(c => c);
            Assert.True(isAllEqual);
        }
    }
}
