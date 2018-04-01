using PKG1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maplestory.io.Data.Items;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using maplestory.io.Data.Images;
using SixLabors.Primitives;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class PetFactory : NeedWZ, IPetFactory
    {
        public Dictionary<int, string> GetPets()
        {
            WZProperty pets = WZ.Resolve("String/Pet") ?? WZ.Resolve("String/Item/Pet");
            return pets.Children.ToDictionary(c => int.Parse(c.NameWithoutExtension), c => c.ResolveForOrNull<string>("name"));
        }

        public Pet GetPet(int petId)
        {
            WZProperty petItem = (WZ.Resolve("String/Pet") ?? WZ.Resolve("String/Item/Pet")).Resolve(petId.ToString());
            return Pet.Parse(petItem);
        }

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
                WZProperty equipPetIdList = WZ.Resolve($"Character/PetEquip/{petEquip.ToString("D8")}");
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
