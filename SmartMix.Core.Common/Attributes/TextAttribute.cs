namespace SmartMix.Core.Common.Attributes
{
    /// <summary>
    /// Задаёт строковое значение.
    /// </summary>
    /// <remarks>Используется как дополнение к атрибуту Description</remarks>
    [AttributeUsage(AttributeTargets.All)]
    public class TextAttribute : Attribute
    {
        public TextAttribute()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public TextAttribute(string value)
        {
            Text = value;
        }

        /// <summary>
        /// Возвращает или задаёт альтернативное описание.
        /// </summary>
        public string Text { get; private set; }
    }
}
