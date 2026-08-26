using SmartMix.Core.Domain.Entities;

namespace SmartMix.Core.Application.AutoStart
{
    public interface IAutoStartRepository
    {
        Task<AutoStartSignal> GetAsync(int lineNumber, int sensorNumber);

        Task ResetAsync(int lineNumber, int sensorNumber);

        Task<bool> IsAutoStartEnabledAsync(int lineNumber, int sensorNumber);
    }
}
