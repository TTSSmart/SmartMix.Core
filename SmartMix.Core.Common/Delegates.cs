using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common
{
    public class Delegates
    {
        public delegate bool TryParseLiteHandler<T>(string value, out T result);

        public delegate bool TryParseHandler<T>(string value, NumberStyles style, IFormatProvider provider, out T result);
    }
}
