using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Extensions
{
    public static class DbCommandExtensions
    {
        public static DbParameter GetParameter(this DbCommand command, string parameterName, DbType dbType, object value)
        {
            DbParameter newParameter = command.CreateParameter();
            return newParameter.SetValues(parameterName, dbType, value);
        }

        public static DbParameter GetParameter(this DbCommand command, string parameterName, int value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, long value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, float value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, double value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, string value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, bool value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, Enum value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter GetParameter(this DbCommand command, string parameterName, DateTime value)
            => command.CreateParameter().SetValues(parameterName, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, DbType dbType, object value)
        {
            dbParameter.ParameterName = $"@{parameterName}";
            dbParameter.DbType = dbType;
            dbParameter.Value = value;
            return dbParameter;
        }

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, int value)
            => dbParameter.SetValues(parameterName, DbType.Int32, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, long value)
            => dbParameter.SetValues(parameterName, DbType.Int64, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, float value)
            => dbParameter.SetValues(parameterName, DbType.Single, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, double value)
            => dbParameter.SetValues(parameterName, DbType.Double, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, string value)
            => dbParameter.SetValues(parameterName, DbType.String, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, bool value)
            => dbParameter.SetValues(parameterName, DbType.Boolean, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, Enum value)
            => dbParameter.SetValues(parameterName, DbType.String, value);

        public static DbParameter SetValues(this DbParameter dbParameter, string parameterName, DateTime value)
            => dbParameter.SetValues(parameterName, DbType.DateTime, value);

        public static DbCommand AddParameter(this DbCommand command, string parameterName, DbType dbType, object value)
        {
            DbParameter parameter = command.GetParameter(parameterName, dbType, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, int value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, long value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, float value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, double value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, string value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, bool value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, Enum value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, string parameterName, DateTime value)
        {
            DbParameter parameter = command.GetParameter(parameterName, value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameterNameId(this DbCommand command, int value)
        {
            DbParameter parameter = command.GetParameter("Id", value);
            return command.AddParameter(parameter);
        }

        public static DbCommand AddParameter(this DbCommand command, DbParameter parameter)
        {
            command.Parameters.Add(parameter);
            return command;
        }
    }
}
