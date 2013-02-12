using System;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Data
{
    public interface ITransactionalDatabaseClient : IDisposable
    {
        IUnitOfWork BeginUnitOfWork();

        Task<IQueryable<T>> Get<T>() where T : class;
        Task Insert<T>(T obj) where T : class;
        //Task Update<T>(T update) where T : class;
        Task Delete<T>(T obj) where T : class;
    }
}
