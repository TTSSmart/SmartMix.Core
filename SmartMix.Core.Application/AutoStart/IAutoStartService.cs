using SmartMix.Core.Domain.Entities;

namespace SmartMix.Core.Application.AutoStart
{
    public interface IAutoStartService
    {
        Task ProcessSignalAsync(
            int lineNumber,
            int sensorNumber);

        Task ResetSignalAsync(
            int lineNumber,
            int sensorNumber);

        Task SetAcknowledgmentAsync(
            int[] applicationIds,
            int mixerSensor,
            int userId);

        Task SetDecisionAsync(
            int[] applicationIds,
            int mixerSensor,
            StartRequestDecision decision,
            int userId);
    }
}
