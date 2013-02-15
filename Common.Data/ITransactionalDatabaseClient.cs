using System;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Data
{
    public interface ITransactionalDatabaseClient : IDisposable, IUnitOfWork
    {
        Task<IQueryable<T>> Get<T>() where T : class;
        void Insert<T>(T obj) where T : class;
        void Update<T>(T update) where T : class;
        void Delete<T>(T obj) where T : class;
    }
}
