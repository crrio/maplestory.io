using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data
{
    public class Android
    {
        public static readonly string StringPath = "Android";

        public Dictionary<string, AndroidMessage[]> ActionMessages;
        public int[] DefaultEquips;
        public int[] PossibleFaces;
        public int[] PossibleHairs;
        public int[] PossibleSkins;
        public int? ChatBalloonStyle;
        public int? Gender;
        public int? NameTagStyle;
        public int Id;

        public static Android Parse(WZProperty data, int id)
        {
            Android result = new Android();
            result.Id = id;

            result.ActionMessages = data.Resolve("action").Children.ToDictionary(c => c.NameWithoutExtension, c => c.Children.Select(b => AndroidMessage.Parse(b)).ToArray());

            result.DefaultEquips = data.Resolve("basic").Children.Select(c => ((WZPropertyVal<int>)c).Value).Where(c => c != 0).ToArray();

            if (data.Children.Any(c => c.NameWithoutExtension.Equals("costume")))
            {
                WZProperty costume = data.Resolve("costume");
                result.PossibleFaces = costume.Resolve("face").Children.Select(c => ((WZPropertyVal<int>)c).Value).Where(c => c != 0).ToArray();
                result.PossibleHairs = costume.Resolve("hair").Children.Select(c => ((WZPropertyVal<int>)c).Value).Where(c => c != 0).ToArray();
                result.PossibleSkins = costume.Resolve("skin").Children.Select(c => ((WZPropertyVal<int>)c).Value).Where(c => c != 0).ToArray();
            }

            if (data.Children.Any(c => c.NameWithoutExtension.Equals("info")))
            {
                WZProperty info = data.Resolve("info");
                result.ChatBalloonStyle = info.ResolveFor<int>("chatBalloon");
                result.Gender = info.ResolveFor<int>("gender");
                result.NameTagStyle = info.ResolveFor<int>("nameTag");
            }

            return result;
        }

        public override string ToString()
            => $"Android - {Id}";
    }

    public class AndroidMessage
    {
        public string Message, Face, Sound;
        public int Probability;

        public static AndroidMessage Parse(WZProperty data)
        {
            AndroidMessage result = new AndroidMessage();

            result.Message = data.ResolveForOrNull<string>("chat");
            result.Face = data.ResolveForOrNull<string>("face");
            result.Sound = data.ResolveForOrNull<string>("sound");
            result.Probability = data.ResolveFor<int>("prob") ?? 100;

            return result;
        }
    }
}
