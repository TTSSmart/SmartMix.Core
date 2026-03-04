namespace SmartMix.Core.Common.Extentions
{
    public static class BitsExtension
    {
        public static bool GetBit(this int value, int bitNumber) => (value & (1 << (bitNumber - 1))) > 0;
    }
}
