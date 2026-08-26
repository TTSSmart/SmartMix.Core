namespace SmartMix.Core.Infrastructure.Persistence.Database.Migration
{
    /// <summary>
    /// Представляет класс описания базового менеджера миграции БД.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class MigrationManagerBase<TConnection, T>
        where TConnection : DbConnection, new()
        where T : DatabaseBase<TConnection>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// </summary>
        /// <param name="databaseObject">Адаптер БД.</param>
        /// <param name="rm">Менеджер ресурсов файлов обновления БД.</param>
        public MigrationManagerBase(T databaseObject, ResourceManager rm)
        {
            Db = databaseObject;
            Resource = rm;
        }

        /// <summary>
        /// Представляет название таблицы миграций.
        /// </summary>
        protected readonly string DbTableName = "migrations";

        /// <summary>
        /// Возвращает или задаёт адаптер БД.
        /// </summary>
        public T Db { get; private set; }

        /// <summary>
        /// Возвращает или задаёт менеджер ресурсов.
        /// </summary>
        public ResourceManager Resource { get; private set; }

        /// <summary>
        /// Выполняет проверку и установку обновлений на БД.
        /// </summary>
        public abstract void Migrate();
    }
}
