namespace SmartMix.Core.Domain.Entities.Base.Shared
{
    public interface ICloneable<T>
    {
        /// <summary>
        /// Создать копию объекта
        /// </summary>
        /// <returns>Клон</returns>
        T Clone();
    }
}
