using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SmartMix.Core.Common.Security
{
    public struct LicenceInfo
    {
        public byte EndHour;
        public byte EndDay;
        public byte EndMonth;
        public byte EndYear;
        public byte Type;
    }

    public static class SerialNumber
    {
        private static byte[] LicenceInfoToByteConverter(LicenceInfo licenceInfo)
        {
            byte[] bytes =
            {
                PackTypeAndHour(licenceInfo.Type, licenceInfo.EndHour),
                0,
                licenceInfo.EndDay,
                licenceInfo.EndMonth,
                licenceInfo.EndYear
            };
            byte control = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                control ^= bytes[i];
            }
            bytes[1] = control;
            return bytes;
        }

        private static LicenceInfo ByteToLicenceInfoConverter(byte[] licenceInfo)
        {
            var licence = new LicenceInfo();
            if (licenceInfo.Length < 5) return licence;
            byte control = 0;
            var bytes = new byte[licenceInfo.Length];
            Array.Copy(licenceInfo, bytes, licenceInfo.Length);
            bytes[1] = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                control ^= bytes[i];
            }
            if (licenceInfo[1] != control) return licence;
            UnPackToTypeAndHour(licenceInfo[0], out licence.Type, out licence.EndHour);
            licence.EndDay = licenceInfo[2];
            licence.EndMonth = licenceInfo[3];
            licence.EndYear = licenceInfo[4];
            return licence;
        }

        /// <summary>
        /// Упаковка значений "тип" и "час" в одну переменную
        /// </summary>
        /// <param name="type">тип (диапазон значений: 0 - 1)</param>
        /// <param name="hour">часы (диапазон значений: 0 - 23)</param>
        /// <returns>возвращает упакованные в 1 байт часы и тип</returns>
        private static byte PackTypeAndHour(byte type, byte hour)
        {
            //Смещаем "hour" на 1 бит ВЛЕВО и добавляем значение первого бита из "type"
            //например, hour = 17, type  = 1
            //hour = 17 = 00010001; result = 00010001 << 1 = 00100010; result = 00100010 + 1 = 00100011 = 35
            byte result = (byte)(hour << 1);
            result = (byte)(result + (byte)(type & 1));

            return result;
        }

        /// <summary>
        /// Распаковка из одной переменной двух значений: "тип" и "час"
        /// </summary>
        /// <param name="variable">байт, содержащий в себе часы и тип в упакованном виде</param>
        /// <param name="type">возвращает тип (диапазон значений: 0 - 1)</param>
        /// <param name="hour">возвращает часы (диапазон значений: 0 - 23)</param>
        private static void UnPackToTypeAndHour(byte variable, out byte type, out byte hour)
        {
            //в переменную "type" заносим значение младшего бита
            //например, variable = 35 = 00100011; type = 1;

            type = (byte)(variable & 1);

            //обнуляем младший бит и смещаем "hour" на 1 бит ВПРАВО
            //hour = 00100011 & 11111110 = 00100010; hour = 00100010 >> 1 = 00010001 = 17
            hour = (byte)(variable & 254);
            hour = (byte)(hour >> 1);
        }

        public static string GenerateKey(LicenceInfo licenceInfo, string fingerPrint)
        {
            IEnumerable<byte> devHash = sha256_hash(fingerPrint);
            var rand = new Random();
            var key = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            rand.NextBytes(key);

            int[] shift = GetShift(devHash);

            byte[] info = LicenceInfoToByteConverter(licenceInfo);
            bool[] infoBitArray = ByteArrayToBoolArray(info);
            int infoBitNumber = 0;
            for (int j = 0; j < shift.Length; j++)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    int bitNumber = GetBitNumber(i, j);
                    if (bitNumber % (shift[j]) == 0)
                    {
                        var mask = (byte)(1 << j);
                        if (infoBitArray.Length > infoBitNumber)
                        {
                            if (infoBitArray[infoBitNumber++])
                            {
                                key[i] = (byte)(key[i] | mask);
                            }
                            else
                            {
                                key[i] = (byte)(key[i] & ~mask);
                            }
                        }
                    }
                }
            }
            return ByteToString(key);
        }

        private static int GetBitNumber(int byteIndex, int bitIndex)
        {
            return byteIndex * 8 + bitIndex;
        }

        private static int[] GetShift(IEnumerable<byte> devHash)
        {
            var shift = new int[8];
            foreach (byte number in devHash)
            {
                for (int j = 0; j < shift.Length; j++)
                {
                    shift[j] += (number & 1 << j) > 0 ? 1 : 0;
                }
            }
            for (int i = 0; i < shift.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    shift[i] = (int)Math.Round(shift[i] / 2.0, MidpointRounding.AwayFromZero);
                }
                if (shift[i] == 0) shift[0]++;
            }
            return shift;
        }

        public static LicenceInfo DecodeSerialNumber(string serialNumber, string fingerPrint)
        {
            try
            {
                byte[] key = StringToBytes(serialNumber);
                byte[] info = DecodeBytes(key, fingerPrint);
                return ByteToLicenceInfoConverter(info);
            }
            catch (Exception)
            {
                return new LicenceInfo
                {
                    EndHour = 0,
                    EndDay = 1,
                    EndMonth = 1,
                    EndYear = 0,
                    Type = 0
                };
            }
        }

        private static byte[] DecodeBytes(byte[] key, string fingerPrint)
        {
            IEnumerable<byte> devHash = sha256_hash(fingerPrint);
            int[] shift = GetShift(devHash);
            int infoBitNumber = 0;
            var resultBitArray = new bool[LicenceInfoToByteConverter(new LicenceInfo()).Length * 8];
            for (int j = 0; j < shift.Length; j++)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    int bitNumber = GetBitNumber(i, j);
                    if (bitNumber % (shift[j]) == 0)
                    {
                        var mask = (byte)(1 << j);

                        if (resultBitArray.Length > infoBitNumber)
                            resultBitArray[infoBitNumber++] = (key[i] & mask) > 0;
                    }
                }
            }

            byte[] resultBytes = BoolArrayToByteArray(resultBitArray);
            return resultBytes;
        }

        private static bool[] ByteArrayToBoolArray(byte[] arr)
        {
            var result = new bool[arr.Length * 8];
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    result[i * 8 + j] = (arr[i] & 1 << j) > 0;
                }
            }
            return result;
        }

        private static byte[] BoolArrayToByteArray(bool[] arr)
        {
            var result = new byte[arr.Length / 8];
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (arr[i * 8 + j])
                        result[i] |= (byte)(1 << j);
                }
            }
            return result;
        }

        private static IEnumerable<byte> sha256_hash(String value)
        {
            byte[] result;
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                result = hash.ComputeHash(enc.GetBytes(value));
            }
            return result;
            //            return new byte[32]
            //            {
            //                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            //                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            //            };
        }

        private static string ByteToString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n = b;
                int n1 = n & 15;
                int n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n2.ToString(CultureInfo.InvariantCulture);
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n1.ToString(CultureInfo.InvariantCulture);
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        private static byte[] StringToBytes(string hex)
        {
            hex = hex.Replace("-", "");
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            var arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }
    }
}
