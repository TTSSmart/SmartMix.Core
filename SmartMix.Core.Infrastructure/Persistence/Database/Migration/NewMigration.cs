namespace SmartMix.Core.Infrastructure.Persistence.Database.Migration
{
    /// <summary>
    /// Представляет класс описания базового скрипта SQL.
    /// </summary>
    internal abstract class Migration
    {
        /// <summary>
        /// Возвращает или задаёт уникальный ID миграции.
        /// Сквозная нумерация значений.
        /// </summary>
        /// <value>ID миграции как ДатаИВремя создания файла в формате yyMMddHHmmss</value>
        public long Id { get; set; }

        /// <summary>
        ///  Возвращает или задаёт наименование запроса миграции.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Представляет класс описания скрипта SQL.
    /// </summary>
    internal class NewMigration : Migration
    {
        /// <summary>
        /// Возвращает или задаёт SQL-запрос [на обновление БД].
        /// </summary>
        public string Sql { get; set; }

        public override string ToString()
        {
            return $"Миграция #{Id}_{Name}";
        }
    }
    //class AppliedMigration : Migration
    //{
    //    public DateTime TimeApplied;
    //}
}
