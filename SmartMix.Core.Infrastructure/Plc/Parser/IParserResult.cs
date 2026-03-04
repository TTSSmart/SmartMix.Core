namespace SmartMix.Core.Infrastructure.Plc.Parser
{
    public interface IParserResult<T>
    {
        ushort StartAddress { get; }
        ushort EndAddress { get; }
        ushort FirstNciAddress { get; }
        Dictionary<string, T> Registers { get; }
    }
}
