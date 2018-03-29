using PKG1;
using System;
using System.Collections.Generic;
using System.Text;

namespace maplestory.io.Data.Mobs
{
    public class Drop
    {
        public bool isMesos;
        public int ItemId;
        public int Min;
        public int Max;
        public bool IsPremium;
        public decimal Probability;

        public Drop () { }

        public Drop(WZProperty drop)
        {
            isMesos = drop.Resolve("money") != null;
            string prob = drop.ResolveForOrNull<string>("prob");

            if (prob.Length > 4)
            {
                prob = prob.Replace("[R8]", "");
                decimal.TryParse(prob, out Probability);
            }

            if (isMesos)
                Min = Max = drop.ResolveFor<int>("money") ?? 0;
            else
            {
                Min = drop.ResolveFor<int>("min") ?? 1;
                Max = drop.ResolveFor<int>("max") ?? 1;
                ItemId = drop.ResolveFor<int>("item") ?? -1;
            }

            IsPremium = drop.ResolveFor<bool>("premium") ?? false;
        }
    }
}
