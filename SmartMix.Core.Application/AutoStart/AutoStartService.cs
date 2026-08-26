using SmartMix.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Application.AutoStart
{
    public sealed class AutoStartService : IAutoStartService
    {
        private readonly IAutoStartRepository _repository;
        private readonly IApplicationStarter _applicationStarter;

        private readonly List<AutoStartRejection> _rejections = new();

        private int _lastApplicationId;

        public AutoStartService(IAutoStartRepository repository, IApplicationStarter applicationStarter)
        {
            _repository = repository;
            _applicationStarter = applicationStarter;
        }

        public Task ProcessSignalAsync(int lineNumber, int sensorNumber)
        {
            throw new NotImplementedException();
        }

        public Task ResetSignalAsync(int lineNumber, int sensorNumber)
        {
            throw new NotImplementedException();
        }

        public Task SetAcknowledgmentAsync(int[] applicationIds, int mixerSensor, int userId)
        {
            throw new NotImplementedException();
        }

        public Task SetDecisionAsync(int[] applicationIds, int mixerSensor, StartRequestDecision decision, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
