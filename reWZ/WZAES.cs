// reWZ is copyright angelsl, 2011 to 2013 inclusive.
// 
// This file (WZAES.cs) is part of reWZ.
// 
// reWZ is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// reWZ is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with reWZ. If not, see <http://www.gnu.org/licenses/>.
// 
// Linking reWZ statically or dynamically with other modules
// is making a combined work based on reWZ. Thus, the terms and
// conditions of the GNU General Public License cover the whole combination.
// 
// As a special exception, the copyright holders of reWZ give you
// permission to link reWZ with independent modules to produce an
// executable, regardless of the license terms of these independent modules,
// and to copy and distribute the resulting executable under terms of your
// choice, provided that you also meet, for each linked independent module,
// the terms and conditions of the license of that module. An independent
// module is a module which is not derived from or based on reWZ.
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace reWZ
{
    internal sealed class WZAES
    {
        internal const uint OffsetKey = 0x581C3F6D;
        private static readonly byte[] AESKey = {0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00};
        private static readonly byte[] GMSIV = {0x4D, 0x23, 0xC7, 0x2B, 0x4D, 0x23, 0xC7, 0x2B, 0x4D, 0x23, 0xC7, 0x2B, 0x4D, 0x23, 0xC7, 0x2B};
        private static readonly byte[] KMSIV = {0xB9, 0x7D, 0x63, 0xE9, 0xB9, 0x7D, 0x63, 0xE9, 0xB9, 0x7D, 0x63, 0xE9, 0xB9, 0x7D, 0x63, 0xE9};

        private readonly byte[] _asciiEncKey;
        private readonly byte[] _asciiKey;
        private readonly byte[] _unicodeEncKey;
        private readonly byte[] _unicodeKey;
        private readonly byte[] _wzKey;

        internal WZAES(WZVariant version)
        {
            _wzKey = GetWZKey(version);
            _asciiKey = new byte[_wzKey.Length];
            _unicodeKey = new byte[_wzKey.Length];
            _asciiEncKey = new byte[_wzKey.Length];
            _unicodeEncKey = new byte[_wzKey.Length];
            unchecked {
                byte mask = 0xAA;
                for (int i = 0; i < _wzKey.Length; ++i, ++mask) {
                    _asciiKey[i] = mask;
                    _asciiEncKey[i] = (byte)(_wzKey[i] ^ mask);
                }
                ushort umask = 0xAAAA;
                for (int i = 0; i < _wzKey.Length/2; i += 2, ++umask) {
                    _unicodeKey[i] = (byte)(umask & 0xFF);
                    _unicodeKey[i+1] = (byte)((umask & 0xFF00) >> 8);
                    _unicodeEncKey[i] = (byte)(_wzKey[i] ^ _unicodeKey[i]);
                    _unicodeEncKey[i + 1] = (byte)(_wzKey[i + 1] ^ _unicodeKey[i + 1]);
                }
            }
        }

        private static byte[] GetWZKey(WZVariant version)
        {
            switch ((int)version) {
                case 0:
                    return GenerateKey(KMSIV, AESKey);
                case 1:
                    return GenerateKey(GMSIV, AESKey);
                case 2:
                    return new byte[0x10000];
                default:
                    throw new ArgumentException("Invalid WZ variant passed.", "version");
            }
        }

        private static byte[] GenerateKey(byte[] iv, byte[] aesKey)
        {
            using (MemoryStream memStream = new MemoryStream(0x10000))
            using (Aes aem = Aes.Create())
            {
                aem.KeySize = 256;
                aem.Key = aesKey;
                aem.Mode = CipherMode.ECB;
                using (CryptoStream cStream = new CryptoStream(memStream, aem.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cStream.Write(iv, 0, 16);
                    for (int i = 0; i < (0x10000 - 16); i += 16)
                        cStream.Write(memStream.ToArray(), i, 16);
                    cStream.Flush();
                    return memStream.ToArray();
                }
            }
        }

        internal string DecryptASCIIString(byte[] asciiBytes, bool encrypted = true)
        {
            int len = asciiBytes.Length;
            if (len > _asciiEncKey.Length)
                throw new NotSupportedException(String.Format("Cannot decrypt ASCII string longer than {0} characters. Please report this!", _asciiEncKey.Length));
            //char[] ret = new char[len];
            byte[] key = encrypted ? _asciiEncKey : _asciiKey;
            for (int i = 0; i < len; ++i)
                asciiBytes[i] ^= key[i];
            return Encoding.ASCII.GetString(asciiBytes);
        }

        internal string DecryptUnicodeString(byte[] ushortChars, bool encrypted = true)
        {
            int len = ushortChars.Length;
            if (len > _unicodeEncKey.Length)
                throw new NotSupportedException(String.Format("Cannot decrypt UTF-16 string longer than {0} characters. Please report this!", _unicodeEncKey.Length));
            //char[] ret = new char[len];
            byte[] key = encrypted ? _unicodeEncKey : _unicodeKey;
            for (int i = 0; i < len; ++i)
                ushortChars[i] ^= key[i];
            return Encoding.Unicode.GetString(ushortChars);
        }

        internal unsafe byte[] DecryptBytes(byte[] bytes)
        {
            fixed (byte* c = bytes, k = _wzKey) {
                byte* d = c, l = k;
                for (int i = 0; i < bytes.Length; ++i)
                    *(d++) ^= *(l++);
            }
            return bytes;
        }
    }

    /// <summary>
    ///   This enum is used to specify the WZ key to be used.
    /// </summary>
    public enum WZVariant
    {
        /// <summary>
        ///   MapleStory SEA
        /// </summary>
        MSEA = 0,

        /// <summary>
        ///   Korea MapleStory
        /// </summary>
        KMS = 0,

        /// <summary>
        ///   Korea MapleStory (Tespia)
        /// </summary>
        KMST = 0,

        /// <summary>
        ///   Japan MapleStory
        /// </summary>
        JMS = 0,

        /// <summary>
        ///   Japan MapleStory (Tespia)
        /// </summary>
        JMST = 0,

        /// <summary>
        ///   Europe MapleStory
        /// </summary>
        EMS = 0,

        /// <summary>
        ///   Global MapleStory
        /// </summary>
        GMS = 1,

        /// <summary>
        ///   Global MapleStory (Tespia)
        /// </summary>
        GMST = 1,

        /// <summary>
        ///   Taiwan MapleStory
        /// </summary>
        TMS = 1,

        /// <summary>
        ///   Brazil MapleStory
        /// </summary>
        BMS = 2,

        /// <summary>
        ///   Classic MapleStory (Data.wz)
        /// </summary>
        Classic = 2
    }
}