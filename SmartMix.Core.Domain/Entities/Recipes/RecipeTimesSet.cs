using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Recipes
{
    /// <summary>Набор времён выгрузки</summary>
    [DataContract]
    public class RecipeTimesSet : BaseInfo, ICloneable<RecipeTimesSet>
    {
        /// <summary>
        /// Максимальное количество дозаторов.
        /// </summary>
        public static readonly int DOSER_COUNT = 12;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public RecipeTimesSet()
        {
            DosersTimes = new int[DOSER_COUNT];
        }

        #region Properties

        /// <summary>
        /// Набор времени задержек по дозаторам, в секундах
        /// </summary>
        [DataMember(IsRequired = true)]
        public int[] DosersTimes { get; set; }

        /// <summary>
        /// Время перемешивания бетонной смеси, в секундах.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int MixTime { get; set; }

        /// <summary>
        /// Возвращает или задаёт признак, привязан ли текущий набор хотя бы к одному рецепту.
        /// Это вычислимое свойство.
        /// </summary>
        /// <value>Значение <see langword="true"/>, если на набор ссылаются рецепты, иначе - значение <see langword="false"/>.</value>
        [DataMember(IsRequired = true)]
        public bool IsUsed { get; set; }

        #endregion Properties

        /// <summary>
        /// Получить задержку по индексу.
        /// </summary>
        /// <param name="index">Индекс дозатора</param>
        /// <returns></returns>
        public int this[int index]
        {
            get { return DosersTimes[index]; }
            set { DosersTimes[index] = value; }
        }

        /// <summary>
        /// Получить время задержки для дозатора с индексом <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Индекс дозатора</param>
        /// <returns>Количество секунд задержки выгрузки (?) дозатора, если значение было найдено, иначе - значение 0/</returns>
        public int GetDoserTime(int index)
        {
            if (index > DOSER_COUNT) return 0;
            return DosersTimes[index - 1];
        }

        /// <summary>
        /// Устанавливает указанный набор времени задержки по времени <paramref name="times"/> на дозаторы.
        /// </summary>
        /// <param name="times">Задержки по дозаторам, в секундах.</param>
        public void SetDoserTimes(int[] times)
        {
            for (int i = 0; i < times.Length && i < DOSER_COUNT; i++)
                DosersTimes[i] = times[i];
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Набор времени {Name} [ID = {Id}]"; // как объект в журнале событий
        }

        #region ICloneable Members

        /// <inheritdoc/>
        public new RecipeTimesSet Clone() => (RecipeTimesSet)MemberwiseClone();

        #endregion ICloneable Members

        ///// <summary>
        ///// Выполняет преобразование типа <see cref="RecipeTimeSetApi"/> к типу <see cref="RecipeTimesSet"/>.
        ///// </summary>
        ///// <param name="model">Источник.</param>
        //public static explicit operator RecipeTimesSet(RecipeTimeSetApi model)
        //{
        //    if (model == null) return null;

        //    return new RecipeTimesSet
        //    {
        //        Id = model.Id,
        //        Name = model.Name,
        //        DosersTimes = model.DosersTimes,
        //        MixTime = model.MixTime,
        //        IsUsed = model.IsUsed
        //    };
        //}
    }
}
