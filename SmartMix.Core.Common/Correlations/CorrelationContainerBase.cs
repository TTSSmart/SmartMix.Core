using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common.Correlations
{
    /// <summary>
    /// Представляет класс для контейнеров сопоставления одного типа к другому.
    /// </summary>
    /// <typeparam name="TBaseTargetType">Базовый класс для объектов сопоставления</typeparam>
    /// <typeparam name="TBaseKey">Базовый класс для ключей</typeparam>
    public abstract class CorrelationContainerBase<TBaseKey, TBaseTargetType>
        where TBaseTargetType : class
        where TBaseKey : class
    {
        /// <summary>
        /// Представляет маппинг типов.
        /// </summary>
        private readonly Dictionary<Type, Type> _correlations;

        /// <summary>
        /// Представляет уникальное название словаря.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным заголовком.
        /// </summary>
        protected CorrelationContainerBase(string name)
        {
            _correlations = new Dictionary<Type, Type>();
            _name = name ?? typeof(TBaseKey).Name;
        }

        /// <summary>
        /// Возвращает название словаря.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Возвращает количество элементов в словаре сопоставлений.
        /// </summary>
        public int Count
        {
            get
            {
                return _correlations.Count;
            }
        }

        /// <summary>
        /// Получить зарегистрированные сопоставления в виде словаря только для чтения.
        /// </summary>
        public ReadOnlyDictionary<Type, Type> GetValues()
        {
            return new ReadOnlyDictionary<Type, Type>(_correlations);
        }

        /// <summary>
        /// Сбрасывает словарь сопоставлений.
        /// </summary>
        public void Clear()
        {
            _correlations.Clear();
        }

        /// <summary>
        /// Зарегистрировать типы соответствия
        /// </summary>
        /// <typeparam name="TKey">Тип ключа</typeparam>
        /// <typeparam name="TTargetType">Тип соответствия ключу</typeparam>
        public void Register<TKey, TTargetType>() where TKey : TBaseKey where TTargetType : TBaseTargetType
        {
            if (_correlations.ContainsKey(typeof(TKey)))
                _correlations[typeof(TKey)] = typeof(TTargetType); // заменили
            else
                _correlations.Add(typeof(TKey), typeof(TTargetType));
        }

        ///// <summary>
        ///// Вернуть экземпляр типа соответствия ключу.
        ///// </summary>
        ///// <param name="key">Ключ, по которому будет создан экземпляр типа соответствия этому ключу.</param>
        ///// <exception cref="InstanceNotFoundException">Инициируется, если под переданным ключом не найден объект.</exception>
        ///// <returns>Возвращает соответствие ключу.</returns>
        //public Type Get(Type key)
        //{
        //    if (_correlations.ContainsKey(key))
        //        return _correlations[key];

        //    throw new InstanceNotFoundException(string.Format(ExceptionResource.TypeMappingNotFound, _name, key));
        //}

        /// <summary>
        /// Получить соответствие типу ключа в виде объекта, созданного через рефлексию по типу значения.
        /// </summary>
        /// <param name="key">Ключ, по которому будет создан экземпляр типа соответствия этому ключу.</param>
        /// <exception cref="InstanceNotFoundException">Инициируется, если под переданным ключом не найден объект.</exception>
        /// <returns>Возвращает соответствие ключу.</returns>
        public TBaseTargetType GetInstance(Type key)
        {
            if (_correlations.ContainsKey(key))
            {
                ConstructorInfo[] ctors = _correlations[key].GetConstructors();
                return (TBaseTargetType)ctors[0].Invoke(null);
            }

            throw new Exception(_name);
            //throw new InstanceNotFoundException(string.Format(ExceptionResource.TypeMappingNotFound, _name, key));
        }

        /// <summary>
        /// Получить соответствие типу ключа в виде объекта созданного через рефлексию по типу значения.
        /// Будет пытаться найти первое соответствие для всего дерева наследования до object.
        /// Вернет первое найденное. либо 
        /// </summary>
        /// <param name="key">Ключ, по которому будет создан экземпляр типа соответствия этому ключу.</param>
        /// <exception cref="InstanceNotFoundException">Инициируется, если под переданным ключом не найден объект.</exception>
        /// <returns>Возвращает соответствие ключу.</returns>
        public TBaseTargetType GetInstanceByРarent(Type key)
        {
            bool success = false;

            Type searchType = key;
            while (!success)
            {
                if (searchType.Equals(typeof(object)))
                {
                    searchType = null; // ничего не нашли
                    break;
                }

                if (_correlations.ContainsKey(searchType))
                    success = true;
                else
                    searchType = searchType.BaseType;
            }

            if (searchType != null)
            {
                ConstructorInfo[] ctors = _correlations[searchType].GetConstructors();
                return (TBaseTargetType)ctors[0].Invoke(null);
            }

            throw new Exception(_name);
            //throw new InstanceNotFoundException(string.Format(ExceptionResource.TypeMappingNotFound, _name, key));
        }
    }
}
