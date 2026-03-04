using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
using SmartMix.Core.Domain.Entities.Consistences;
using SmartMix.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Domain.Entities.Recipes
{
    /// <summary>
    /// Рецепт
    /// </summary>
    [DataContract]
    public class Recipe : BaseInfo, ICloneable<Recipe>, IEquatable<Recipe>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public Recipe() : base()
        {
            _1CGuid = Guid.Empty;
            _1CNumber = string.Empty;

            RecipeTimesSets = new List<Dictionary<string, int>>();
            RecipeCategory = new RecipeCategory();

            Structures = new List<RecipeStructure>();

            UseAutoCorrection = true;

            MixerSetId = 1; // "Базовый набор"
            RecipeMixerSet = new RecipeMixerSet() { Id = MixerSetId };

            TimeSetId = 1; // "Базовый набор"
            RecipeTimesSet = new RecipeTimesSet() { Id = TimeSetId };
            //TableConsistency = new HeaderConsistency() { Id = -1, Name = string.Empty };

            CalibLevelHumidity = new Dictionary<LevelHumidity, RecipeHumidity>()
            {
                { LevelHumidity.Min, new RecipeHumidity() },
                { LevelHumidity.Middle, new RecipeHumidity() },
                { LevelHumidity.Max, new RecipeHumidity() }
            };

            OnUpdateId += UpdateId; // уйти на INotifyPropertyChanged
        }

        #region Properties

        /// <summary>
        /// ID набора времен выгрузки
        /// </summary>
        /// <remarks>Избыточное свойство, так как дублирует <see cref="RecipeTimesSet"/>.</remarks>
        [DataMember(IsRequired = true)]
        public int TimeSetId { get; set; }

        /// <summary>
        /// учитываемый/неучитываемый рецепт
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool Consider { get; set; } // todo о чём это??

        /// <summary>
        /// Использовать автокорректировку
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool UseAutoCorrection { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        [DataMember(IsRequired = true)]
        public int UserId { get; set; } // todo расширить по BaseObject ModifiedBy

        /// <summary>
        /// Дата и время последних изменений по рецепту
        /// </summary>
        [DataMember(IsRequired = true)]
        public DateTime EditDate { get; set; }

        /// <summary>
        /// Набор времен выгрузки
        /// </summary>
        /// <remarks>Используется в заявках.</remarks>
        [DataMember(IsRequired = true)]
        public RecipeTimesSet RecipeTimesSet { get; set; }

        /// <summary>
        /// Словарь калибровок влажности
        /// </summary>
        [DataMember(IsRequired = true)]
        public Dictionary<LevelHumidity, RecipeHumidity> CalibLevelHumidity { get; set; }

#pragma warning disable IDE1006 // Стили именования
        /// <summary>
        /// Уникальный внешний идентификатор рецепта типа <see cref="Guid"/>.
        /// Значение по умолчанию: <see cref="Guid.Empty"/>.
        /// </summary>
        [DataMember(IsRequired = true)]
        public Guid _1CGuid { get; set; }

        /// <summary>
        /// Номер рецепта в 1С.
        /// Значение по умолчанию: <see cref="string.Empty"/>.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string _1CNumber { get; set; }
#pragma warning restore IDE1006 // Стили именования

        /// <summary>ID набора импульсов для затворов миксера</summary>
        /// <remarks>Избыточное свойство, так как дублирует <see cref="RecipeMixerSet"/>.</remarks>
        [DataMember]
        public int MixerSetId { get; set; }

        /// <summary>Набор работы затворов для смесителя</summary>
        /// <remarks>Исп. в заявках</remarks>
        [DataMember(IsRequired = true)]
        public RecipeMixerSet RecipeMixerSet { get; set; }

        /// <summary>
        /// Зависимость консистенции(таблица консистенций)
        /// </summary>
        [DataMember(IsRequired = true)]
        public HeaderConsistency TableConsistency { get; set; }

        /// <summary>
        /// Наборы времен
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<Dictionary<string, int>> RecipeTimesSets { get; set; } // todo ??

        /// <summary>
        /// Категория рецепта
        /// </summary>
        [DataMember(IsRequired = true)]
        public RecipeCategory RecipeCategory { get; set; }

        /// <summary>
        /// Состав рецепта
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<RecipeStructure> Structures { get; set; }

        #endregion Properties

        private void UpdateId()
        {
            if (Structures != null)
            {
                for (int i = 0; i < Structures.Count; i++)
                    Structures[i].RecipeId = Id;
            }
        }

        #region ICloneble Members

        /// <inheritdoc/>
        public new Recipe Clone()
        {
            Recipe clone = (Recipe)MemberwiseClone();
            clone.RecipeCategory = RecipeCategory.Clone();
            clone.Structures = Structures.ConvertAll(s => s.Clone());
            clone.RecipeTimesSet = RecipeTimesSet?.Clone();

            clone.CalibLevelHumidity = CalibLevelHumidity.ToDictionary(k => k.Key, v => v.Value.Clone());

            return clone;
        }

        #endregion ICloneble Members

        #region IEquatable Members

        /// <inheritdoc/>
        /// <remarks>Используется для корректировок.</remarks>
        public bool Equals(Recipe other)
        {
            if (other == null)
                return false;

            if (this.Id == other.Id)
            {
                foreach (var otherStructure in other.Structures)
                {
                    foreach (var thisStructure in this.Structures)
                    {
                        if (otherStructure.ComponentId == thisStructure.ComponentId)
                        {
                            if (otherStructure.Correct != thisStructure.Correct)
                                return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        #endregion IEquatable Members

        /// <summary>
        /// Приводит базовый объект к рецепту.
        /// </summary>
        /// <param name="baseInfo">Входной объект</param>
        /// <returns>Рецепт</returns>
        public static Recipe ConvertFrom(BaseInfo baseInfo)
        {
            if (baseInfo == null)
                return null;

            return new Recipe()
            {
                Id = baseInfo.Id,
                OldId = baseInfo.OldId,
                Name = baseInfo.Name
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        //public static explicit operator Recipe(RecipeApi model)
        //{
        //    if (model == null) return null;

        //    Recipe obj = new Recipe()
        //    {
        //        Consider = true,
        //        Id = model.Id,
        //        Name = model.Name,
        //        UseAutoCorrection = model.UseAutoCorrection,
        //        UserId = model.UserId
        //    };

        //    if (Guid.TryParse(model._1CGuid, out Guid guid))
        //        obj._1CGuid = guid;
        //    obj._1CNumber = model._1CNumber ?? string.Empty;

        //    obj.RecipeCategory = (RecipeCategory)model.RecipeCategory;

        //    obj.Structures = model.Structures.ConvertAll(x => (RecipeStructure)x);

        //    if (DateTime.TryParse(model.EditDate, out DateTime dt))
        //        obj.EditDate = dt;

        //    obj.MixerSetId = model.MixerSetId == 0 ? 1 : model.MixerSetId;
        //    obj.TableConsistency = new HeaderConsistency() { Id = model.ConsistencyTable, Name = string.Empty };

        //    obj.RecipeTimesSet = (RecipeTimesSet)model.RecipeTimesSet;
        //    obj.TimeSetId = (model.RecipeTimesSet == null || model.RecipeTimesSet.Id == 0) ? 1 : model.RecipeTimesSet.Id;

        //    //
        //    return obj;
        //}
    }
}
