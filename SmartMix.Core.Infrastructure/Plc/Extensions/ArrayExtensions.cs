namespace SmartMix.Core.Infrastructure.Plc.Extensions
{
    public static class ArrayExtensions
    {
        public static ushort[] ToArrayUshorts(this byte[] bytes)
        {
            var sdata = new ushort[bytes.Length / 2];

            for (int i = 0, j = 0; i < bytes.Length; i += 2, j++)
            {
                sdata[j] = (ushort)((ushort)(bytes[i] << 8) + bytes[i + 1]);
            }
            return sdata;
        }

        public static byte[] ToArrayBytes(this ushort[] ushorts)
        {
            var result = new byte[ushorts.Length * sizeof(ushort)];

            for (int i = 0, j = 0; i < ushorts.Length; i++, j += 2)
            {
                result[j + 1] = (byte)(ushorts[i] & 0xff);
                result[j] = (byte)(ushorts[i] >> 8);
            }
            return result;
        }
    }
}
