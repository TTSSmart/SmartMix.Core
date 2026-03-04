using SmartMix.Core.Domain.Entities.Base.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Domain.Entities.Base
{
    /// <summary>Базовая информация по объекту.</summary>
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    [DataContract]
    public class BaseInfo : ICloneable<BaseInfo>, INotifyPropertyChanged
    {
        /// <summary>
        /// Событие Обновление Id
        /// </summary>
        public Action OnUpdateId {  get; set; }

        /// <summary>
        /// Представляет уникальный идентификатор объекта в таблице.
        /// </summary>
        private int _id = -1;

        /// <summary>Возвращает или задаёт уникальный идентификатор объекта в таблице.</summary>
        [DataMember(IsRequired = true)]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();

                    OnUpdateId?.Invoke();
                }
            }
        }

        /// <summary>
        /// Представляет название объекта.
        /// </summary>
        private string _name = string.Empty;

        /// <summary>Название объекта в таблице </summary>
        [DataMember(IsRequired = true)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>Ссылка на базовый объект из справочника</summary>
        [DataMember]
        public int OldId { get; set; }

        ///// <summary>
        ///// Приводит модель типа <see cref="BaseInfoApi"/> к объекту SmartMix <see cref="BaseInfo"/>.
        ///// </summary>
        ///// <param name="model">Базовая модель SmartMix REST API.</param>
        //public static explicit operator BaseInfo(BaseInfoApi model)
        //{
        //    if (model == null) return null;

        //    return new BaseInfo()
        //    {
        //        Id = model.Id,
        //        Name = model.Name
        //    };
        //}

        /// <summary>
        /// Приводит базовый объект <paramref name="baseObject"/> к объекту типа <typeparamref name="TObject"/>.
        /// </summary>
        /// <param name="baseObject">Базовый объект SmartMix</param>
        public static TObject ConvertFrom<TObject>(BaseInfo baseObject)
            where TObject : BaseInfo, new()
        {
            if (baseObject == null) return null;

            return new TObject()
            {
                Id = baseObject.Id,
                Name = baseObject.Name,
                OldId = baseObject.OldId
            };
        }

        #region ICloneable Members

        /// <inheritdoc/>
        public BaseInfo Clone()
        {
            return (BaseInfo)MemberwiseClone();
        }

        #endregion ICloneable Members

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged

    }
}
