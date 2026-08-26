namespace SmartMix.Core.Infrastructure.Persistence.Database.Config
{
    using MySql.Data.MySqlClient;

    /// <summary>
    /// Конфигурация подключения к базе данных
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// Сервер базы данных: IP-Адрес/Хост
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Имя базы данных
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Порт для подключения
        /// </summary>
        /// <value>Значение по умолчанию: 3306 для MySQL</value>
        public int Port { get; set; }

        /// <summary>
        /// Выполняет построение строки подключения MySQL. 
        /// Возвращает результат операции.
        /// </summary>
        /// <returns>Строка подключения к БД MySQL.</returns>
        public string ToMySqlConnectionString()
        {
            return new MySqlConnectionStringBuilder()
            {
                Server = Address,
                Database = DatabaseName,
                UserID = User,
                Password = Password,
                Port = (uint)Port,
                MaximumPoolSize = 50,
                MinimumPoolSize = 2,
                Pooling = true,
                AllowUserVariables = true,
                CharacterSet = "cp1251"
            }
            .ToString();
        }

        #region IEquality Members

        public override bool Equals(object obj)
        {
            DatabaseConfig y = obj as DatabaseConfig;

            if (this == null && y == null)
                return true;
            if (this == null || y == null)
                return false;
            if (Object.ReferenceEquals(this, y))
                return true;

            return string.Equals(Address, y.Address, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(DatabaseName, y.DatabaseName, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(User, y.User, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(Password, y.Password)
                && Port == y.Port;
        }

        public override int GetHashCode()
        {
            int hCode = (this.Address ?? string.Empty).GetHashCode()
                ^ (this.DatabaseName ?? string.Empty).GetHashCode()
                ^ (this.User ?? string.Empty).GetHashCode()
                ^ (this.Password ?? string.Empty).GetHashCode()
                ^ this.Port.GetHashCode();
            return hCode.GetHashCode();
        }

        #endregion IEquality Members
    }
}
