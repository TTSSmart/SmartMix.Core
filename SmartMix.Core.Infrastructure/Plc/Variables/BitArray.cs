using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Plc.Variables
{
    public struct BitArray : IEquatable<BitArray>
    {
        private ushort[] _value;

        public BitArray(int size)
        {
            _value = new ushort[size];
        }
        public BitArray(ushort[] value)
        {
            _value = value;
        }

        public int CountBits => _value.Length * 16;

        public bool GetBitValue(int bitNumber)
        {
            bitNumber = bitNumber.Normalize(out int offset);

            return ((_value[offset] & (1 << bitNumber)) > 0);
        }

        public void SetBitValue(int bitNumber, bool value)
        {
            bitNumber = bitNumber.Normalize(out int offset);

            lock (_value)
            {
                ushort val = _value[offset];

                if (value)
                {
                    val = (ushort)(val | (1 << bitNumber));
                }
                else
                {
                    val = (ushort)(val & ~(1 << bitNumber));
                }
                _value[offset] = val;
            }
        }

        public ushort[] GetCopyArray()
        {
            var res = new ushort[_value.Length];
            _value.CopyTo(res, 0);
            return res;
        }

        /// <summary>
        /// Возвращает строку
        /// </summary>
        /// <returns>строка</returns>
        public string GetString()
        {
            string str = "";
            for (int i = 0; i < _value.Length; i++)
            {
                if (_value[i] == 0) break;
                byte[] b = BitConverter.GetBytes(_value[i]);
                str += Encoding.GetEncoding(1251).GetString(b);
            }
            str = str.Trim(new Char[] { '\0' });
            return str;
        }

        public bool Equals(BitArray other)
        {
            if (_value == null)
                return false;

            return other._value.SequenceEqual(_value);
        }
    }

    public static class NormalizeHelper
    {
        public static int Normalize(this int bitN, out int offset)
        {
            offset = bitN / 16;
            int bitinReg = bitN % 16 - 1;

            if (bitinReg < 0)
            {
                offset--;
                bitinReg = 15;
            }
            return bitinReg;
        }
    }
}
