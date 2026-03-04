namespace SmartMix.Core.Infrastructure.Plc.Extensions
{
    public static class Int32Extensions
    {
        public static int Int32Normalize(this int bitN, out int offset)
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
