using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Events
{
    /// <summary>Класс описания события из журнала событий.</summary>
    /// <remarks>см. таблицу events</remarks>
    [DataContract]
    public class FullEvent : BaseEvent, INotifyPropertyChanged
    {
        /// <summary>Уникальный код события</summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Представляет статус [аварийного] события.
        /// </summary>
        private EventStatus _status;

        /// <summary>
        /// Возвращает или задаёт статус [аварийного] события.
        /// </summary>
        [DataMember]
        public EventStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    RaisePropertyChanged(nameof(Status));
                }
            }
        }

        /// <summary>Объект связанный с данным событием</summary>
        [DataMember(IsRequired = true)]
        public BaseObject Object { get; set; }

        /// <summary>Тип события</summary>
        [DataMember(IsRequired = true)]
        public EventType Type { get; set; }

        /// <summary>Пользователь</summary>
        [DataMember(IsRequired = true)]
        public BaseObject User { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members
    }
}
