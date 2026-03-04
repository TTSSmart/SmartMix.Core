using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
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
    /// Влажность рецепта
    /// </summary>
    [DataContract]
    public class RecipeHumidity : BaseInfo, ICloneable<RecipeHumidity>
    {
        public RecipeHumidity()
        {
            MixerHumidityKoef = 1;
        }

        /// <summary>
        /// Актуальный номер рецепта
        /// </summary>
        [DataMember(IsRequired = true)]
        public int RecipeId { get; set; }

        /// <summary>
        /// Уровень объёма рецепта 
        /// </summary>
        [DataMember(IsRequired = true)]
        public LevelHumidity CalibLevelHumidity { get; set; }

        /// <summary>
        /// Влажность
        /// </summary>
        [DataMember(IsRequired = true)]
        public float MixerHumidity { get; set; }

        /// <summary>
        /// Коэффициент влажности
        /// </summary>
        [DataMember(IsRequired = true)]
        public float MixerHumidityKoef { get; set; }

        /// <summary>
        /// объём замеса
        /// </summary>
        [DataMember(IsRequired = true)]
        public float VolumeBatch { get; set; }

        /// <summary>
        ///  Шаг
        /// </summary>
        [DataMember(IsRequired = true)]
        public float Steep { get; set; }

        /// <summary>
        ///  Набор
        /// </summary>
        [DataMember(IsRequired = true)]
        public float Ofset { get; set; }

        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public new RecipeHumidity Clone()
        {
            return (RecipeHumidity)MemberwiseClone();
        }
    }
}
