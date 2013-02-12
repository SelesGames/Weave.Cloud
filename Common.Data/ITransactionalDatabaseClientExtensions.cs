using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Common.Data
{
    public static class ITransactionalDatabaseClientExtensions
    {
        public static Task<T> GetSingle<T>(this ITransactionalDatabaseClient client, Expression<Func<T, bool>> selector)
            where T : class
        {
            return client.Get<T>().ContinueWith(
                task =>
                {
                    var result = task.Result;
                    return result.SingleOrDefault(selector);
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public static Task<IQueryable<T>> GetAllByCriteria<T>(this ITransactionalDatabaseClient client, Expression<Func<T, bool>> selector)
            where T : class
        {
            return client.Get<T>().ContinueWith(
                task =>
                {
                    var result = task.Result;
                    return result.Where(selector);
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
