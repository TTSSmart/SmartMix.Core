using System;
using System.Collections.Generic;
using System.Data.Common;


namespace SmartMix.Core.Infrastructure.Persistence.Database.Extensions
{
    public static class DbConnectionExtensions
    {
        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            return command;
        }
    }
}
