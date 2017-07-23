using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData
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
            if (itemString.Children.ContainsKey("name")) return null;

            return new ItemDescription(
                itemId,
                itemString.ResolveForOrNull<string>("name"),
                itemString.ResolveForOrNull<string>("desc")
            );
        }
    }
}
