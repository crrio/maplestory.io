using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data
{
    public class ItemDescription
    {
        public int Id;
        public string Name, Description;
        public ItemDescription(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public static ItemDescription Parse(WZProperty itemString, int itemId)
        {
            if (!itemString.Children.Any(c => c.NameWithoutExtension.Equals("name"))) return null;

            return new ItemDescription(
                itemId,
                itemString.ResolveForOrNull<string>("name"),
                string.Join("", itemString.ResolveForOrNull<string>("desc") ?? "", itemString.ResolveForOrNull<string>("autodesc") ?? "")
            );
        }
    }
}
