using System;
using System.Collections.Generic;
using System.Text;
using maplestory.io.Data.Images;
using maplestory.io.Data.Items;
using System.Linq;

namespace maplestory.io.Data.Characters
{
    public class EquipEntry
    {
        public Equip Equip;
        public string Action;
        public int? Frame;

        public IEnumerable<EquipFrameEntry> GetFrameEntry(int weaponCategory, string characterAction)
        {
            Dictionary<string, EquipFrameBook> books = Equip.GetFrameBooks(weaponCategory);

            string actionUsed = Action ?? characterAction ?? "default";
            EquipFrameBook book = null;
            if (books.ContainsKey(actionUsed))
            {
                book = books[actionUsed];

                if (book.frames.Count() < ((Frame ?? 0) % book?.frames.Count() ?? 0))
                    book = null;
            }

            if (book == null && books.ContainsKey("default"))
            {
                book = books["default"];
                actionUsed = "default";
            }

            EquipFrame frame = book?.frames.ElementAt((Frame ?? 0) % book?.frames.Count() ?? 0);

            return frame?.Effects?.Select(c => new EquipFrameEntry()
            {
                Position = c.Key,
                Action = actionUsed,
                Equip = Equip,
                SelectedFrame = c.Value
            }) ?? Enumerable.Empty<EquipFrameEntry>();
        }

        public IEnumerable<EquipFrameEntry> GetEffectFrameEntry(string characterAction)
        {
            Dictionary<string, IEnumerable<FrameBook>> books = Equip.ItemEffects.entries;
            IEnumerable<FrameBook> book = null;
            string actionUsed = Action ?? characterAction;

            if (books.ContainsKey(actionUsed))
            {
                book = books[actionUsed];

                if (book?.Count() > 0)
                    book = null;
            }

            if (book == null && books.ContainsKey("default")) {
                book = books["default"];
                actionUsed = "default";
            }

            return book.Select(c =>
            {
                IFrame frame = c.frames.ElementAt((Frame ?? 0) % c.frames.Count());
                return new EquipFrameEntry()
                {
                    Action = actionUsed,
                    Equip = Equip,
                    SelectedFrame = frame
                };
            }).GroupBy(c => c.SelectedFrame.Position).Select(c => c.FirstOrDefault());
        }
    }
}
