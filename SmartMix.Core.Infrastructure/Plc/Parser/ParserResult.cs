namespace SmartMix.Core.Infrastructure.Plc.Parser
{
    public class ParserResult<T> : IParserResult<T>
    {
        public ParserResult(
            ushort startAddress,
            ushort endAddress,
            ushort firstNciAddress,
            Dictionary<string, T> registers)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
            FirstNciAddress = firstNciAddress;
            Registers = registers;
        }

        public ushort StartAddress { get; }
        public ushort EndAddress { get; }
        public ushort FirstNciAddress { get; }
        public Dictionary<string, T> Registers { get; }
    }
}
