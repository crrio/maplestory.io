using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace maplestory.io.Models
{
    public class CharacterLook
    {
        private static byte[] ms_abIV = { 17, 23, 0xCD, 16, 4, 63, 0x8E, 122, 18, 21, 0x80, 17, 93, 25, 79, 16 };
        private static byte[] ms_abKey = { 16, 4, 63, 17, 23, 0xCD, 18, 21, 93, 0x8E, 122, 25, 0x80, 17, 79, 20 };

        private static int typeMultiplier = 10000;
        private static int genderMultiplier = 1000;

        static Tuple<Action<CharacterLook, int>, int>[] bitUnpacking = new Tuple<Action<CharacterLook, int>, int>[]
        {
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.someCheck = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.skinId = (sbyte)val, 4),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.faceId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.faceGender = (byte)val, 3),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.isHairOver40000 = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.hairId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.hairGender = (byte)val, 4),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.capId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.capGender = (byte)val, 3),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.faceAccessoryId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.faceAccessoryGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.eyeAccessoryId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.eyeAccessoryGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.earAccessoryId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.earAccessoryGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.isLongcoat = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.coatId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.coatGender = (byte)val, 4),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.pantsId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.pantsGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.shoesId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.shoesGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.gloveId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.gloveGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.capeId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.capeGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.isNotBlade = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.isSubweapon = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.shieldId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.shieldGender = (byte)val, 4),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.isCashWeapon = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.weaponId = (short)val, 10),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.weaponGender = (byte)val, 2),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.weaponType = (byte)val, 5),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance.isElfEar = val == 1, 1),
            new Tuple<Action<CharacterLook, int>, int>((instance, val) => instance._unknown2 = (short)val, 12)
        };

        static Tuple<Func<CharacterLook, int>, int>[] bitPacking = new Tuple<Func<CharacterLook, int>, int>[]
        {
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.someCheck ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.skinId, 4),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.faceId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.faceGender, 3),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.isHairOver40000 ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.hairId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.hairGender, 4),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.capId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.capGender, 3),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.faceAccessoryId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.faceAccessoryGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.eyeAccessoryId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.eyeAccessoryGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.earAccessoryId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.earAccessoryGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.isLongcoat ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.coatId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.coatGender, 4),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.pantsId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.pantsGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.shoesId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.shoesGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.gloveId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.gloveGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.capeId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.capeGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.isNotBlade ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.isSubweapon ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.shieldId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.shieldGender, 4),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.isCashWeapon ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.weaponId, 10),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.weaponGender, 2),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.weaponType, 5),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance.isElfEar ? 1 : 0, 1),
            new Tuple<Func<CharacterLook, int>, int>((instance) => instance._unknown2, 12)
        };

        bool someCheck = false; // 1bit
        sbyte skinId = -1; // 4bit
        short faceId = -1; // 10bit
        byte faceGender = 0; // 3bit
        bool isHairOver40000 = false; // 1bit
        short hairId = -1; // 10bit
        byte hairGender = 0; // 4bit
        short capId = -1; // 10bit
        byte capGender = 0; // 3bit
        short faceAccessoryId = -1; // 10bit
        byte faceAccessoryGender = 0; // 2bit
        short eyeAccessoryId = -1; // 10bit
        byte eyeAccessoryGender = 0; // 2bit
        short earAccessoryId = -1; // 10bit
        byte earAccessoryGender = 0; // 2bit
        bool isLongcoat = false; // 1bit
        short coatId = -1; // 10bit
        byte coatGender = 0; // 4bit
        short pantsId = -1; // 10bit
        byte pantsGender = 0; // 2bit
        short shoesId = -1; // 10bit
        byte shoesGender = 0; // 2bit
        short gloveId = -1; // 10bit
        byte gloveGender = 0; // 2bit
        short capeId = -1; // 10bit
        byte capeGender = 0; // 2bit
        bool isNotBlade = false; // 1bit
        bool isSubweapon = false; // 1bit
        short shieldId = -1; // 10bit
        byte shieldGender = 0; // 4bit
        bool isCashWeapon = false; // 1bit
        short weaponId = -1; // 10bit
        byte weaponGender = 0; // 2bit
        byte weaponType = 0; // 5bit
        bool isElfEar = false; // 1bit
        short _unknown2 = 0xB0; // 12bit

        public CharacterLook() { }

        public CharacterLook(string packedCharacterLook)
        {
            using (Aes aes = Aes.Create()) {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.Key = ms_abKey;
                aes.IV = ms_abIV;

                using (ICryptoTransform decrypt = aes.CreateDecryptor())
                {
                    byte[] data = Enumerable.Range(0, packedCharacterLook.Length / 2).Select(c => packedCharacterLook.Substring(c * 2, 2)).Select(c => (byte)(((c[0] - 'A') << 4) | (c[1] - 'A'))).ToArray();
                    byte[] decrypted = decrypt.TransformFinalBlock(data, 0, data.Length);

                    int offset = 0;
                    foreach (Tuple<Action<CharacterLook, int>, int> bitUnpack in bitUnpacking)
                    {
                        short value = 0;
                        for (int i = 0; i < bitUnpack.Item2; i += 1)
                            if ((decrypted[(offset + i) / 8] & (1 << ((offset + i) % 8))) != 0)
                                value |= (short)(1 << i);
                        bitUnpack.Item1(this, value);
                        offset += bitUnpack.Item2;
                    }
                }
            }
        }

        public string Pack()
        {
            byte[] packedCharacterLook = new byte[128];
            int offset = 0;
            foreach(Tuple<Func<CharacterLook, int>, int> pack in bitPacking)
            {
                for (int i = 0; i < pack.Item2; i += 1)
                {
                    int value = pack.Item1(this);
                    if ((value & (1 << i)) != 0)
                    {
                        packedCharacterLook[(offset + i) / 8] |= (byte)(1 << ((offset + i) % 8));
                    }
                }
                offset += pack.Item2;
            }

            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.Key = ms_abKey;
                aes.IV = ms_abIV;

                using (ICryptoTransform encrypt = aes.CreateEncryptor())
                {
                    byte[] decrypted = encrypt.TransformFinalBlock(packedCharacterLook, 0, packedCharacterLook.Length);
                    string data = string.Join("", decrypted.Select(c => new string(new char[] { (char)((c >> 4) + 'A'), (char)((c & 0xF) + 'A') })));
                    return data;
                }
            }
        }

        private static int getId(int type, int gender, int id)
        {
            return type * typeMultiplier + gender * genderMultiplier + id;
        }

        public int Skin
        {
            get
            {
                if (skinId == -1)
                {
                    return -1;
                }
                return skinId;
            }
            set
            {
                skinId = (sbyte)value;
            }
        }

        public int Face
        {
            get
            {
                if (faceId == -1 || faceId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.face, faceGender, faceId);
            }
            set
            {
                faceId = (short)(value % genderMultiplier);
                faceGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Hair
        {
            get
            {
                if (hairId == -1 || hairId == 0x3FF)
                {
                    return -1;
                }
                if (isHairOver40000)
                {
                    return getId(ItemType.hair2, hairGender, hairId);
                }
                return getId(ItemType.hair, hairGender, hairId);
            }
            set
            {
                isHairOver40000 = (value / typeMultiplier == ItemType.hair2);
                hairId = (short)(value % genderMultiplier);
                hairGender = (byte)((value / genderMultiplier) % 10);
            }
        }


        public int Cap
        {
            get
            {
                if (capId == -1 || capId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.cap, capGender, capId);
            }
            set
            {
                capId = (short)(value % genderMultiplier);
                capGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int FaceAccessory
        {
            get
            {
                if (faceAccessoryId == -1 || faceAccessoryId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.faceAccessory, faceAccessoryGender, faceAccessoryId);
            }
            set
            {
                faceAccessoryId = (short)(value % genderMultiplier);
                faceAccessoryGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int EyeAccessory
        {
            get
            {
                if (eyeAccessoryId == -1 || eyeAccessoryId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.eyeAccessory, eyeAccessoryGender, eyeAccessoryId);
            }
            set
            {
                eyeAccessoryId = (short)(value % genderMultiplier);
                eyeAccessoryGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int EarAccessory
        {
            get
            {
                if (earAccessoryId == -1 || earAccessoryId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.earAccessory, earAccessoryGender, earAccessoryId);
            }
            set
            {
                earAccessoryId = (short)(value % genderMultiplier);
                earAccessoryGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Coat
        {
            get
            {
                if (coatId == -1 || coatId == 0x3FF)
                {
                    return -1;
                }
                if (isLongcoat)
                {
                    return getId(ItemType.longcoat, coatGender, coatId);
                }
                return getId(ItemType.coat, coatGender, coatId);
            }
            set
            {
                isLongcoat = (value / typeMultiplier == ItemType.longcoat);
                coatId = (short)(value % genderMultiplier);
                coatGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Pants
        {
            get
            {
                if (pantsId == -1 || pantsId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.pants, pantsGender, pantsId);
            }
            set
            {
                pantsId = (short)(value % genderMultiplier);
                pantsGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Shoes
        {
            get
            {
                if (shoesId == -1 || shoesId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.shoes, shoesGender, shoesId);
            }
            set
            {
                shoesId = (short)(value % genderMultiplier);
                shoesGender = (byte)((value / genderMultiplier) % 10);
            }

        }

        public int Glove
        {
            get
            {
                if (gloveId == -1 || gloveId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.glove, gloveGender, gloveId);
            }
            set
            {
                gloveId = (short)(value % genderMultiplier);
                gloveGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Cape
        {
            get
            {
                if (capeId == -1 || capeId == 0x3FF)
                {
                    return -1;
                }
                return getId(ItemType.cape, capeGender, capeId);
            }
            set
            {
                capeId = (short)(value % genderMultiplier);
                capeGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Shield
        {
            get
            {
                if (shieldId == -1 || shieldId == 0x3FF)
                {
                    return -1;
                }
                if (!isNotBlade)
                {
                    return getId(ItemType.blade, shieldGender, shieldId);
                }
                if (isSubweapon)
                {
                    return getId(ItemType.subweapon, shieldGender, shieldId);
                }
                return getId(ItemType.shield, shieldGender, shieldId);
            }
            set
            {
                isNotBlade = (value / typeMultiplier != ItemType.blade);
                isSubweapon = (value / typeMultiplier == ItemType.subweapon);
                shieldId = (short)(value % genderMultiplier);
                shieldGender = (byte)((value / genderMultiplier) % 10);
            }
        }

        public int Weapon
        {
            get
            {
                if (weaponId == -1 || weaponId == 0x3FF || ItemType.getFullWeaponType(weaponType) == -1)
                {
                    return -1;
                }
                if (isCashWeapon)
                {
                    return getId(ItemType.cashWeapon, weaponGender, weaponId);
                }
                return getId(ItemType.getFullWeaponType(weaponType), weaponGender, weaponId);
            }
            set
            {
                isCashWeapon = (value / typeMultiplier == ItemType.cashWeapon);
                weaponId = (short)(value % genderMultiplier);
                weaponGender = (byte)((value / genderMultiplier) % 10);
                weaponType = ItemType.getByteWeaponType(value / typeMultiplier);
            }
        }

        static class ItemType
        {
            public static int face = 2;
            public static int hair = 3;
            public static int hair2 = 4;
            public static int cap = 100;
            public static int faceAccessory = 101;
            public static int eyeAccessory = 102;
            public static int earAccessory = 103;
            public static int coat = 104;
            public static int longcoat = 105;
            public static int pants = 106;
            public static int shoes = 107;
            public static int glove = 108;
            public static int cape = 110;
            public static int shield = 109;
            public static int blade = 134;
            public static int subweapon = 135;
            public static int cashWeapon = 170;
            public static int[] weapons = { -1, 130, 131, 132, 133, 137, 138, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, -1, 134, 152, 153, -1, 136, 121, 122, 123, 124, 156, 157, 126, 158, 127 };

            public static int getFullWeaponType(byte weaponType)
            {
                if (weaponType < 0 || weaponType >= weapons.Length)
                {
                    return -1;
                }
                return weapons[weaponType];
            }

            public static byte getByteWeaponType(int weaponType)
            {
                for (int i = 0; i < weapons.Length; i += 1)
                {
                    if (weaponType == weapons[i])
                    {
                        return (byte)i;
                    }
                }
                return 0xFF;
            }
        }
    }
}