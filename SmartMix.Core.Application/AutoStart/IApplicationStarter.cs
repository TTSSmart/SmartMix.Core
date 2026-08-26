
namespace SmartMix.Core.Application.AutoStart
{
    public interface IApplicationStarter
    {
        Task StartAsync(
            int[] applicationIds,
            int userId);
    }
}
