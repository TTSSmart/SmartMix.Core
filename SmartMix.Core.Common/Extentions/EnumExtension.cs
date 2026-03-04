using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common.Extentions
{
    public static class EnumExtention
    {
        /// <summary>
        /// Получить параметр атрибута <see cref="DescriptionAttribute"/>
        /// </summary>
        /// <param name="value">Значение перечисления</param>
        /// <returns></returns>
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            return GetAttributeValue(fi) ?? value.ToString();
        }

        /// <summary>
        /// Получить список <see cref="DescriptionAttribute"/> всех значений
        /// </summary>
        /// <param name="value">Значение перечисления</param>
        /// <returns></returns>
        public static string[] GetEnumsDescriprion(this Type enumType)
        {
            var res = new List<string>();
            foreach (FieldInfo item in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                res.Add((GetAttributeValue(item)) ?? item.Name);
            }

            return res.ToArray();
        }

        /// <summary>
        /// Получить список <see cref="DescriptionAttribute"/> всех значений
        /// </summary>
        /// <param name="value">Значение перечисления</param>
        /// <returns></returns>
        public static (int, string)[] GetEnumsIndexAndDescriprion(this Type enumType)
        {
            var res = new List<(int, string)>();
            foreach (FieldInfo item in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                res.Add(((int)item.GetValue(null), (GetAttributeValue(item)) ?? item.Name));
            }

            return res.ToArray();
        }

        /// <param name="fi"></param>
        /// <returns>null - если атрибут не обнаружен</returns>
        private static string GetAttributeValue(FieldInfo fi)
        {
            return fi.GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attribute ? attribute.Description : null;

        }

        public static T GetEnumForDescription<T>(this Type enumType, string description) where T : struct, Enum
        {
            foreach (FieldInfo item in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (GetAttributeValue(item) != null && GetAttributeValue(item).Equals(description))
                    return (T)Enum.Parse(enumType, item.Name);
            }
            throw new Exception($"В перечислении {enumType} не найдено значение с Description = {description}");
        }

        public static T ToEnum<T>(this string value, T defaultValue = default)
          where T : struct
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            T result;
            return Enum.TryParse<T>(value, out result) ? result : defaultValue;
        }
    }
}
