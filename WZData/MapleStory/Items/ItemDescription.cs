using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData
{
    public class ItemDescription
    {
        public int Id;
        public string Name, Description;
        internal string WZFile, WZFolder;
        public ItemDescription(int id, string name, string description, string file, string folder)
        {
            Id = id;
            Name = name;
            Description = description;
            WZFile = file;
            WZFolder = folder;
        }

        public static ItemDescription Parse(WZObject child, string path)
        {
            int itemId = -1;
            if (!int.TryParse(child.Name, out itemId) || !child.HasChild("name"))
                return null;

            string wzFile = "";
            string folder = "";

            if (path.StartsWith("Eqp.img/Eqp"))
            {
                wzFile = "Character.wz";
                folder = path.Substring(11) + $"/{itemId.ToString("D8")}.img";
            }
            else
            {
                wzFile = "Item.wz";

                switch (path)
                {
                    case "Cash.img":
                        folder = $"Cash/{itemId.ToString("D8").Substring(0,4)}.img/{itemId.ToString("D8")}";
                        break;
                    case "Consume.img":
                        folder = $"Consume/{itemId.ToString("D8").Substring(0, 4)}.img/{itemId.ToString("D8")}";
                        break;
                    case "Etc.img/Etc":
                        folder = $"Etc/{itemId.ToString("D8").Substring(0, 4)}.img/{itemId.ToString("D8")}";
                        break;
                    case "Ins.img":
                        folder = $"Install/{itemId.ToString("D8").Substring(0, 4)}.img/{itemId.ToString("D8")}";
                        break;
                    case "Pet.img":
                        folder = $"Pet/{itemId.ToString()}.img";
                        break;
                }
            }

            WZStringProperty nameContainer = (WZStringProperty)child.ResolvePath("name");

            string desc = "";
            if (child.HasChild("desc") && !(child["desc"] is WZNullProperty))
            {
                WZStringProperty descContainer = (WZStringProperty)child.ResolvePath("desc");
                desc = descContainer.Value;
            }

            string name = nameContainer.Value;

            return new ItemDescription(itemId, name, desc, wzFile, folder);
        }
    }
}
