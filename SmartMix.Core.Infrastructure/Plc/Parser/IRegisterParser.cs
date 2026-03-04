namespace SmartMix.Core.Infrastructure.Plc.Parser
{
    public interface IRegisterParser<T>
    {
        IParserResult<T> GetFromCsvText(string text, ushort start);
    }
}
