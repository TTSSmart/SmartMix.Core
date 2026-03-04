using SmartMix.Core.Domain.Entities.Base.Shared;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Recipes.Components
{
    [DataContract]
    public class TypeComp : ICloneable<TypeComp>, IComparable<TypeComp>
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }

        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        /// <inheritdoc/>
        public TypeComp Clone()
        {
            return new TypeComp
            {
                Id = Id,
                Name = Name
            };
        }

        public int CompareTo(TypeComp other) => Id > other.Id ? 1 : 0;
    }
}
