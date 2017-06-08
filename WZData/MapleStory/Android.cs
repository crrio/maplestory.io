using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory
{
    public class Android
    {
        public static readonly string StringPath = "Android";

        public Dictionary<string, AndroidMessage[]> ActionMessages;
        public int[] DefaultEquips;
        public int[] PossibleFaces;
        public int[] PossibleHairs;
        public int[] PossibleSkins;
        public int ChatBalloonStyle;
        public int Gender;
        public int NameTagStyle;
        public int Id;

        public static Android Parse(WZObject data, int id)
        {
            Android result = new Android();
            result.Id = id;

            if (data.HasChild("action"))
                result.ActionMessages = data["action"].ToDictionary(c => c.Name, c => c.Select(AndroidMessage.Parse).ToArray());

            if (data.HasChild("basic"))
                result.DefaultEquips = data["basic"].Select(c => c.ValueOrDefault<int>(0)).Where(c => c != 0).ToArray();

            if (data.HasChild("costume"))
            {
                if (data["costume"].HasChild("face"))
                    result.PossibleFaces = data["costume"]["face"].Select(c => c.ValueOrDefault(0)).Where(c => c != 0).ToArray();

                if (data["costume"].HasChild("hair"))
                    result.PossibleHairs = data["costume"]["hair"].Select(c => c.ValueOrDefault(0)).Where(c => c != 0).ToArray();

                if (data["costume"].HasChild("skin"))
                    result.PossibleSkins = data["costume"]["skin"].Select(c => c.ValueOrDefault(0)).Where(c => c != 0).ToArray();
            }

            if (data.HasChild("info"))
            {
                result.ChatBalloonStyle = data["info"].HasChild("chatBalloon") ? data["info"]["chatBalloon"].ValueOrDefault<int>(0) : 0;
                result.Gender = data["info"].HasChild("gender") ? data["info"]["gender"].ValueOrDefault<int>(0) : 0;
                result.NameTagStyle = data["info"].HasChild("nameTag") ? data["info"]["nameTag"].ValueOrDefault<int>(0) : 0;
            }

            return result;
        }

        public static IEnumerable<Tuple<int, Func<Android>>> GetLookup(WZFile etcWz)
        {
            int id = -1;
            foreach (WZObject item in etcWz.ResolvePath(StringPath))
                if (int.TryParse(item.Name.Replace(".img", ""), out id))
                    yield return new Tuple<int, Func<Android>>(id, CreateLookup(item, id).Memoize());
        }

        private static Func<Android> CreateLookup(WZObject androidImg, int id)
            => ()
            => Parse(androidImg, id);

        public override string ToString()
            => $"Android - {Id}";
    }

    public class AndroidMessage
    {
        public string Message, Face, Sound;
        public int Probability;

        public static AndroidMessage Parse(WZObject data)
        {
            AndroidMessage result = new AndroidMessage();

            result.Message = data.HasChild("chat") ? data["chat"].ValueOrDefault("") : null;
            result.Face = data.HasChild("face") ? data["face"].ValueOrDefault("") : null;
            result.Sound = data.HasChild("sound") ? data["sound"].ValueOrDefault("") : null;
            result.Probability = data.HasChild("prob") ? data["prob"].ValueOrDefault(100) : 100;

            return result;
        }
    }
}
