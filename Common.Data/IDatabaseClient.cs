using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Data
{
    public interface IDatabaseClient<T>
    {
        Task<T> GetSingle(Func<T, bool> selector);
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAllByCriteria(Func<T, bool> selector);
        Task Insert(T obj);
        Task<TId> Insert<TId>(T obj, Func<T, TId> idSelector);
        Task Update(T update);
        Task Delete(T obj);
    }
}
