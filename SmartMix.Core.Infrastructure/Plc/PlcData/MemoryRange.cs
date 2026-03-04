using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Plc.PlcData
{
    internal class MemoryRange
    {
        private int _blockSize;

        public int StartAddress { get; set; }
        public int EndAddress { get; set; }

        public int BlockSize
        {
            get { return _blockSize; }
            set
            {
                if (value < 1)
                {
                    _blockSize = 1;
                }
                else if (value > 122)
                {
                    _blockSize = 122;
                }
                else
                {
                    _blockSize = value;
                }
            }
        }
    }
}
