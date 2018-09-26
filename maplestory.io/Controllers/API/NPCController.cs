using maplestory.io.Data.Characters;
using maplestory.io.Data.Images;
using maplestory.io.Data.NPC;
using maplestory.io.Data.Quests;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace maplestory.io.Controllers
{
    [Route("api/{region}/{version}/npc")]
    public class NPCController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult List([FromQuery] int startAt = 0, [FromQuery]int count = int.MaxValue, [FromQuery]string searchFor = "") => Json(NPCFactory.GetNPCs(startAt, count, searchFor));

        [Route("{npcId}")]
        [HttpGet]
        public IActionResult GetNPC(int npcId)
        {
            NPC npcInfo = NPCFactory.GetNPC(npcId);
            npcInfo.RelatedQuestsInfo = npcInfo.RelatedQuests?.Select(c => QuestFactory.GetQuest(c)).Where(c => c != null).Select(c => new QuestName() { id = c.Id, name = c.Name }).ToArray();
            return Json(npcInfo);
        }

        [Route("{npcId}/icon")]
        [HttpGet]
        public IActionResult GetFrame(int npcId)
        {
            NPC npcData = NPCFactory.GetNPC(npcId);

            if (npcData.IsComponentNPC ?? false)
            {
                return File(AvatarFactory.Render(new Character()
                {
                    AnimationName = "stand1",
                    ItemEntries = npcData.ComponentIds
                        .Concat(new int[] { npcData.ComponentSkin ?? 2000, (npcData.ComponentSkin ?? 2000) + 10000 })
                        .Select(c => new AvatarItemEntry() { ItemId = c, Region = Region, Version = Version })
                        .ToArray()
                }).ImageToByte(Request), "image/png");
            }
            if (!npcData.Framebooks.ContainsKey("stand")) return NotFound();

            FrameBook standing = npcData.GetFrameBook("stand").First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.First();
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }

        [Route("{npcId}/name")]
        [HttpGet]
        public IActionResult GetName(int npcId)
        {
            NPC npcData = NPCFactory.GetNPC(npcId);
            return Json(new
            {
                Name = npcData.Name,
                Id = npcData.Id
            });
        }

        [Route("{npcId}/render/{framebook}/{frame?}")]
        [HttpGet]
        public IActionResult Render(int npcId, string framebook, int frame = 0)
        {
            NPC npcData = NPCFactory.GetNPC(npcId);

            if (npcData.IsComponentNPC ?? false)
            {
                return File(AvatarFactory.Render(new Character()
                {
                    AnimationName = framebook,
                    FrameNumber = frame,
                    ItemEntries = npcData.ComponentIds
                        .Concat(new int[] { npcData.ComponentSkin ?? 2000, (npcData.ComponentSkin ?? 2000) + 10000 })
                        .Select(c => new AvatarItemEntry() { ItemId = c, Region = Region, Version = Version })
                        .ToArray()
                }).ImageToByte(Request), "image/png");
            }

            FrameBook standing = npcData.GetFrameBook(framebook).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.ElementAt(frame % standing.frames.Count());
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }
    }
}