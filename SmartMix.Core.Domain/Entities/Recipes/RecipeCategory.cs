using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartMix.Core.Domain.Entities.Recipes
{
    public class RecipeCategory : BaseInfo, ICloneable<RecipeCategory>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public RecipeCategory() : base()
        {
            ParentCategoryId = -1;

            _1CGuid = Guid.Empty;
            _1CNumber = string.Empty;
        }

        #region Properties

        /// <summary>
        /// Идентификатор родительской категории.
        /// При значении -1, категория является основной (главной), иначе - это подкатегория.
        /// </summary>
        /// <value>Значение -1 для основной категории.</value>
        [DataMember(IsRequired = true)]
        public int ParentCategoryId { get; set; }

        /// <summary>
        /// Идентификатор из внешней системы типа GUID.
        /// </summary>
        /// <value>Значение по умолчанию: <see cref="Guid.Empty"/></value>
        [DataMember]
        public Guid _1CGuid { get; set; }

        /// <summary>
        /// Внешний номер из 1С
        /// </summary>
        /// <value>Значение по умолчанию: <see cref="string.Empty"/></value>
        [DataMember]
        public string _1CNumber { get; set; }

        #endregion Properties

        #region ICloneable Members

        /// <inheritdoc/>
        public RecipeCategory Clone()
        {
            return (RecipeCategory)this.MemberwiseClone();
        }

        #endregion ICloneable Members

        /// <inheritdoc/>
        //public static explicit operator RecipeCategory(RecipeCategoryApi model)
        //{
        //    if (model == null) return null;

        //    RecipeCategory obj = new RecipeCategory();
        //    obj.Id = model.Id;
        //    obj.Name = model.Name;
        //    obj.ParentCategoryId = model.ParentCategoryId;

        //    if (Guid.TryParse(model._1CGuid, out Guid guid))
        //        obj._1CGuid = guid;
        //    obj._1CNumber = model._1CNumber ?? string.Empty;

        //    return obj;
        //}

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(BaseInfo.Id)} = {Id}, {nameof(BaseInfo.Name)} = {Name}, {nameof(ParentCategoryId)} = {ParentCategoryId}";
        }
    }
}
