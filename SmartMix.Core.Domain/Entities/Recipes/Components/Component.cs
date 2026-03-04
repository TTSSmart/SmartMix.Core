using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Domain.Entities.Recipes.Components
{
    /// <summary>
    /// Класс описания компонента.
    /// </summary>
    [DataContract]
    public class Component : BaseInfo, ICloneable<Component>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public Component() : base()
        {
            Humidity = float.NaN;
            Density = 1;

            _1CGuid = Guid.Empty;
            _1CNumber = string.Empty;
        }

        #region Properties

        /// <summary>Влажность материала</summary>
        [DataMember(IsRequired = true)]
        public float Humidity { get; set; }

        /// <summary>Засорённость материала</summary>
        [DataMember(IsRequired = true)]
        public float Impurity { get; set; }

        /// <summary>Плотность материала, кг/л</summary>
        /// <value>Значение по умолчанию: 1 кг/л</value>
        [DataMember(IsRequired = true)]
        public float Density { get; set; }

        /// <summary>Идентификатор типа компонента</summary>
        /// <value>Числовое значение <see cref="ComponentType"/>.</value>
        [DataMember(IsRequired = true)]
        public int IdType { get; set; } // todo избыточное свойство

        /// <summary>
        /// Представляет информацию по типу компонента.
        /// </summary>
        private TypeComp _typeComp;

        /// <summary>
        /// Возвращает или задаёт информацию по типу компонента.
        /// </summary>
        [DataMember(IsRequired = true)]
        public TypeComp TypeComp
        {
            get
            {
                return _typeComp;
            }
            set
            {
                _typeComp = value;
                if (value != null)
                    IdType = value.Id;
            }
        }

        /// <summary>Признак удалённой записи</summary>
        [DataMember(IsRequired = true)]
        public bool Deleted { get; set; }

        /// <summary>Вычислимое свойство "Найдены ссылки на компонент в таблице рецептов".</summary>
        [DataMember(IsRequired = true)]
        public bool UsedInRecipes { get; set; }

        /// <summary>
        /// GUID из 1С
        /// </summary>
        ///<value>Значение по умолчанию: <see cref="Guid.Empty"/></value>
        [DataMember]
        public Guid _1CGuid { get; set; }

        /// <summary>
        /// Номер из 1С
        /// </summary>
        /// <value>Значение по умолчанию: <see cref="string.Empty"/></value>
        [DataMember]
        public string _1CNumber { get; set; }

        #endregion Properties

        #region ICloneable Members

        /// <inheritdoc/>
        public Component Clone()
        {
            var clone = (Component)MemberwiseClone();
            clone.TypeComp = _typeComp != null ? _typeComp.Clone() : null;

            return clone;
        }

        #endregion ICloneable Members

        /// <inheritdoc/>
        //public static explicit operator Component(ComponentApi model)
        //{
        //    if (model == null)
        //        return null;

        //    var component = new Component();
        //    component.Id = model.Id;
        //    component.Name = model.Name;
        //    component.Humidity = model.Humidity;
        //    component.Impurity = model.Impurity;
        //    component.Density = model.Density;
        //    component.Deleted = model.Deleted;
        //    component.UsedInRecipes = model.UsedInRecipes;

        //    component.TypeComp = model.TypeComp;

        //    component._1CGuid = Guid.TryParse(model._1CGuid ?? string.Empty, out Guid guid) ? guid : Guid.Empty;
        //    component._1CNumber = model._1CNumber ?? string.Empty;

        //    return component;
        //}
    }
}
