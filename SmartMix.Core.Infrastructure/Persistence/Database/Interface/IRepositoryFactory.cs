using System.Data.Common;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Interface
{
    public interface IRepositoryFactory
    {
        void Add<TInput, TOutput>();
        TOutput Get<TOutput>();
        void InitializeDb<T>(string connectionString) where T : DbConnection, new();
    }
}
