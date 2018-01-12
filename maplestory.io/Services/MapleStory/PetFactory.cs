using PKG1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Items;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using WZData.MapleStory.Images;
using SixLabors.Primitives;

namespace maplestory.io.Services.MapleStory
{
    public class PetFactory : NeedWZ<IPetFactory>, IPetFactory
    {
        static Dictionary<int, Pet> cache = new Dictionary<int, Pet>();

        public PetFactory(IWZFactory factory) : base(factory) { }
        public PetFactory(IWZFactory _factory, Region region, string version) : base(_factory, region, version) { }

        public Dictionary<int, string> GetPets()
        {
            WZProperty pets = wz.Resolve("String/Pet") ?? wz.Resolve("String/Item/Pet");
            return pets.Children.ToDictionary(c => int.Parse(c.Key), c => c.Value.ResolveForOrNull<string>("name"));
        }

        public Pet GetPet(int petId)
        {
            if (!cache.ContainsKey(petId))
            {
                WZProperty item = (wz.Resolve("String/Pet") ?? wz.Resolve("String/Item/Pet")).Resolve(petId.ToString());
                try
                {
                    if (!cache.ContainsKey(petId))
                        cache.Add(petId, Pet.Parse(item));
                }
                catch (Exception) { } // Usually happens when multi threaded caching something
            }
            return cache[petId];
        }

        public override IPetFactory GetWithWZ(Region region, string version)
            => new PetFactory(_factory, region, version);

        public Image<Rgba32> RenderPet(int petId, string animation, int frame, int petEquip)
        {
            Pet eq = GetPet(petId);

            Frame[] frames = eq.frameBooks[animation].First().frames.ToArray();
            int frameCount = frames.Count();
            int realFrame = (frame % frameCount);
            Frame petFrame = frames[realFrame];

            if (petEquip != -1)
            {
                Point origin = petFrame.OriginOrZero;
                WZProperty equipPetIdList = wz.Resolve($"Character/PetEquip/{petEquip.ToString("D8")}");
                WZProperty equipNode = equipPetIdList?.Resolve(petId.ToString())?.Resolve()?.Resolve($"{animation}/{realFrame}")?.Resolve();
                if (equipNode == null) return petFrame.Image;

                Frame equipFrame = Frame.Parse(equipNode);
                Point equipOrigin = equipFrame.OriginOrZero;
                Point renderEquipAt = new Point(origin.X - equipOrigin.X, origin.Y - equipOrigin.Y);

                int minX = Math.Min(renderEquipAt.X, 0);
                int minY = Math.Min(renderEquipAt.Y, 0);
                int maxX = Math.Max(renderEquipAt.X + equipFrame.Image.Width, petFrame.Image.Width);
                int maxY = Math.Max(renderEquipAt.Y + equipFrame.Image.Height, petFrame.Image.Height);

                Image<Rgba32> result = new Image<Rgba32>(maxX - minX, maxY - minY);
                result.Mutate(x =>
                {
                    x.DrawImage(petFrame.Image, 1, new Size(petFrame.Image.Width, petFrame.Image.Height), new Point(0 - minX, 0 - minY));
                    x.DrawImage(equipFrame.Image, 1, new Size(equipFrame.Image.Width, equipFrame.Image.Height), new Point(renderEquipAt.X - minX, renderEquipAt.Y - minY));
                });
                return result;
            }
            else return petFrame.Image;
        }
    }
}
