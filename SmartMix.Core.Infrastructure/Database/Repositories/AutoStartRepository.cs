using SmartMix.Core.Application.AutoStart;
using SmartMix.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Database.Repositories
{
    public sealed class AutoStartRepository : IAutoStartRepository
    {
        public async Task<AutoStartSignal> GetAsync(
                        int lineNumber,
                        int sensorNumber)
        {
            // Пока здесь вызываем старый Database.Instance
            // Позже заменим непосредственно на новый DAL.

            return await Database.Database.Instance
                .GetAutoSignalAsync(lineNumber, sensorNumber);
        }

        public Task ResetAsync(
            int lineNumber,
            int sensorNumber)
        {
            return Database.Database.Instance
                .ResetAutoSignalAsync(lineNumber, sensorNumber);
        }

        public Task<bool> IsAutoStartEnabledAsync(
            int lineNumber,
            int sensorNumber)
        {
            return Task.FromResult(
                Database.Database.Instance
                    .CheckAutoSignal(lineNumber, sensorNumber));
        }
    }
