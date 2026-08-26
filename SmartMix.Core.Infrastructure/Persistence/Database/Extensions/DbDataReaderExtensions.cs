using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Extensions
{
    public static class DbDataReaderExtensions
    {
        public static T Convert<T>(this DbDataReader reader, string columnName) where T : struct
        {
            int original = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(original))
                return default(T);

            return reader.GetFieldValue<T>(original);

        }

        public static int ConvertInt32(this DbDataReader reader, string columnName)
        {
            int original = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(original))
                return default(int);

            return reader.GetInt32(original);
        }
    }
}
