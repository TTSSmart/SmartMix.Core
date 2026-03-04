namespace SmartMix.Core.Common.Helpers
{
    public static class NameGenerator
    {
        /// <summary>
        /// Сгенерировать новое имя по шаблону <paramref name="template"/> на основе порядкового номера.
        /// </summary>
        /// <param name="template">Шаблон имени.</param>
        /// <param name="existsNames">Набор уже использованных имён.</param>
        /// <returns>Новое название по шаблону.</returns>
        public static string GenerateNewName(string template, string[] existsNames)
        {
            Array.Sort(existsNames);

            int startIndex = 0;
            int counter = 1;
            for (int i = startIndex; i < existsNames.Length; i++)
            {
                for (int j = 0; j < existsNames.Length; j++)
                {
                    string newName = string.Format("{0} {1}", template, counter);
                    if (existsNames[j] == newName)
                        counter++;
                }
                startIndex++;
            }

            return string.Format("{0} {1}", template, counter);
        }

        /// <summary>
        /// Выполняет проверку наименования элемента <paramref name="value"/> и возвращает имя по шаблону.
        /// </summary>
        /// <param name="value">Название копируемого элемента.</param>
        /// <param name="template">Шаблон имени по умолчанию.</param>
        /// <returns>Обработанное название для последующего копирования.</returns>
        public static string CheckName(string value, string template)
        {
            if (string.IsNullOrEmpty(value))
                return template;

            //if (string.Equals(value, Resources.LocalizationAddResource.UndefinedListItemName))
            //    return template;

            return value;
        }
    }
}
