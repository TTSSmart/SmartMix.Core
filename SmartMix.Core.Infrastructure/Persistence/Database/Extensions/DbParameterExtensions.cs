using System.Data.Common;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Extensions
{
    public static class DbParameterExtensions
    {
        public static string ToForString(this DbParameterCollection parameters)
        {
            string result = "";
            foreach (DbParameter item in parameters)
                result += $"{item.ParameterName}={item.Value},";

            return result;
        }
    }
}
