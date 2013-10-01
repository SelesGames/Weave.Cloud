using System;
using System.Linq;

namespace Common.Data
{
    public interface ITransactionalDatabaseClient : IDisposable, IUnitOfWork
    {
        IQueryable<T> Get<T>() where T : class;
        void Insert<T>(T obj) where T : class;
        void Update<T>(T obj) where T : class;
        void Delete<T>(T obj) where T : class;
    }
}
