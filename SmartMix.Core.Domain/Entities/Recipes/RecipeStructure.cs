using SmartMix.Core.Domain.Entities.Base.Shared;
using SmartMix.Core.Domain.Entities.Recipes.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Domain.Entities.Recipes
{
    /// <summary>
    /// Класс описания элемента структуры рецепта. 
    /// </summary>
    [DataContract]
    public class RecipeStructure : ICloneable<RecipeStructure>
    {
        #region Properties

        /// <summary>Идентификатор рецепта</summary>
        [DataMember(IsRequired = true)]
        public int RecipeId { get; set; }

        /// <summary>Идентификатор компонента</summary>
        [DataMember(IsRequired = true)]
        public int ComponentId { get; set; }  // todo избыточное свойство наряду с Component

        /// <summary>Наименование компонента</summary>
        [DataMember(IsRequired = true)]
        public string ComponentName { get; set; }

        /// <summary>Вес/количество/объём</summary>
        [DataMember(IsRequired = true)]
        public float Amount { get; set; }

        /// <summary>Пересчитанное значение с корректировкой по влажности</summary>
        [DataMember(IsRequired = true)]
        public float RecalculatedAmount { get; set; }

        /// <summary>Пересчитанное значение с корректировкой по плотности</summary>
        [DataMember(IsRequired = true)]
        public float RecalculatedAmount2 { get; set; }

        /// <summary>
        /// Порядок дозирования компонента в рамках одного дозатора. 
        /// Первым будет дозироваться компонент с меньшим значением <see cref="MixOrder"/>.
        /// В редакторе рецептов порядок дозирования компонентов идёт слева направо.
        /// </summary>
        /// <remarks>При значении 0 компоненты будут проиндексированы автоматически.</remarks>
        [DataMember(IsRequired = true)]
        public int MixOrder { get; set; }

        /// <summary>Корректировка, в кг</summary>
        [DataMember(IsRequired = true)]
        public float Correct { get; set; }

        /// <summary>
        /// Флаг изменения корректировки
        /// </summary>
        [DataMember]
        public bool IsSetCorrect { get; set; } // todo так работать не будет

        /// <summary>Информация по компоненту/материалу.</summary>
        [DataMember(IsRequired = true)]
        public Component Component { get; set; } // todo избыточное свойство наряду с ComponentId?

        #endregion Properties

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public RecipeStructure()
        {
            Component = new Component();
        }

        /// <inheritdoc/>
        public RecipeStructure Clone()
        {
            return new RecipeStructure
            {
                RecipeId = RecipeId,
                ComponentId = ComponentId,
                ComponentName = ComponentName,
                Amount = Amount,
                RecalculatedAmount = RecalculatedAmount,
                RecalculatedAmount2 = RecalculatedAmount2,
                MixOrder = MixOrder,
                Correct = Correct,
                Component = Component?.Clone()
            };
        }
    }
}
