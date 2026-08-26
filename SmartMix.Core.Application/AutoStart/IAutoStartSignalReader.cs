namespace SmartMix.Core.Application.AutoStart
{
    public interface IAutoStartSignalReader
    {
        bool GetSignalState(int sensorNumber);
    }
}
