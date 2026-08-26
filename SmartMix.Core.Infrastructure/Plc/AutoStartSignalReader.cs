using SmartMix.Core.Application.AutoStart;
using SmartMix.Core.Infrastructure.Plc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Plc
{
    public sealed class AutoStartSignalReader : IAutoStartSignalReader
    {
        private readonly IPlcIO _plc;

        public AutoStartSignalReader(IPlcIO plc)
        {
            _plc = plc;
        }

        public bool GetSignalState(int sensorNumber)
        {
            return _plc.GetArrayVariable(PlcVars.PlcVarsPatterns.Nvo.SensorStatus).Value.GetBitValue(sensorNumber);
        }
    }
}
