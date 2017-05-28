using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Models.Market
{
    public class LegacyItem
    {
        public short t;
        public short u;
        public int b;
        public short k;
        public short l;
        public short x;
        public short m;
        public short q;
        public short s;
        public short n;
        public short o;
        public sbyte i;
        public long c;
        public int a;
        public short w;
        public short j;
        public sbyte h;
        public short p;
        public short r;
        public byte d;
        public int e;
        public string f;
        public string g;
        public short v;
        public sbyte y;
        [JsonProperty("B")]
        public short B;
        [JsonProperty("C")]
        public sbyte C;
        [JsonProperty("Y")]
        public bool Y;
        [JsonProperty("Q")]
        public string Q;
        [JsonProperty("R")]
        public string R;
        [JsonProperty("S")]
        public string S;
        [JsonProperty("E")]
        public string E;
        [JsonProperty("P")]
        public string P;
        [JsonProperty("A")]
        public int A;
        [JsonProperty("T")]
        public int T;
        [JsonProperty("D")]
        public sbyte D;
        [JsonProperty("F")]
        public byte F;
        [JsonProperty("O")]
        public string O;
        [JsonProperty("V")]
        public short V;
        [JsonProperty("H")]
        public sbyte H;
        [JsonProperty("G")]
        public ItemRarity G;
        [JsonProperty("U")]
        public int U;
        [JsonProperty("I")]
        public string I;
        [JsonProperty("J")]
        public string J;
        [JsonProperty("K")]
        public string K;
        [JsonProperty("L")]
        public string L;
        [JsonProperty("M")]
        public string M;
        [JsonProperty("N")]
        public string N;


        public LegacyItem() { }

        public LegacyItem(WorldItem that) {
            this.t = that.acc;
            this.u = that.avoid;
            this.B = that.battleModeAtt;
            this.C = that.bossDmg;
            this.b = that.bundle;
            this.Q = that.OverallCategory;
            this.R = that.category;
            this.S = that.SubCategory;
            this.E = that.creator;
            this.P = that.description;
            this.k = that.dex;
            this.v = that.diligence;
            this.y = that.growth;
            this.A = that.hammerApplied;
            this.T = that.itemId;
            this.D = that.ignoreDef;
            this.l = that.intelligence;
            this.F = that.isIdentified;
            this.x = that.jump;
            this.m = that.luk;
            this.q = that.matk;
            this.s = that.mdef;
            this.n = that.mhp;
            this.o = that.mmp;
            this.O = that.name;
            this.V = that.nebulite;
            this.H = that.numberOfEnhancements;
            this.i = that.numberOfPlusses;
            this.c = that.price;
            this.a = that.quantity;
            this.G = that.rarity;
            this.w = that.speed;
            this.j = that.str;
            this.h = that.upgradesAvailable;
            this.p = that.watk;
            this.r = that.wdef;
            this.U = that.itemId;
            this.d = that.channel;
            this.e = that.room;
            this.f = that.shopName;
            this.g = that.characterName;
            if (that.potentials?.Length > 0)
            {
                this.I = that.potentials[0]?.line;
                this.J = that.potentials[1]?.line;
                this.K = that.potentials[2]?.line;
                this.L = that.potentials[3]?.line;
                this.M = that.potentials[4]?.line;
                this.N = that.potentials[5]?.line;
            }
            this.Y = that.cash?.cash ?? false;
        }
    }
}
