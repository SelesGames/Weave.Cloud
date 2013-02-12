using Common.Validation;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Data.Linq
{
    public abstract class LinqDbClient
    {
        readonly string connectionString;

        public LinqDbClient(SqlServerCredentials credentials)
        {
            this.connectionString = credentials.GetConnectionString();
        }

        protected DataContext CreateContext()
        {
#if DEBUG
            return new DataContext(connectionString) { ObjectTrackingEnabled = true, Log = new DebugTextWriter() };
#else
            return new DataContext(connectionString) { ObjectTrackingEnabled = true };
#endif
        }

        protected DataContext CreateReadOnlyContext()
        {
#if DEBUG
            return new DataContext(connectionString) { ObjectTrackingEnabled = false, Log = new DebugTextWriter() };
#else
            return new DataContext(connectionString) { ObjectTrackingEnabled = true };
#endif
        }
    }

    public class LinqDbClient<T> : LinqDbClient, IDatabaseClient<T> where T : class
    {
        public LinqDbClient(SqlServerCredentials credentials) : base(credentials) { }

        public ValidationEngine ValidationEngine { get; set; }

        public async Task<T> GetSingle(Func<T, bool> selector)
        {
            var result = await Task.Run(() =>
            {
                using (var context = CreateReadOnlyContext())
                {
                    var table = context.GetTable<T>();
                    var o = table.SingleOrDefault(selector);
                    return o;
                }
            });
            return result;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            var result = await Task.Run(() =>
            {
                using (var context = CreateReadOnlyContext())
                {
                    var table = context.GetTable<T>();
                    var o = table.ToList();
                    return o;
                }
            });
            return result;
        }

        public async Task<IEnumerable<T>> GetAllByCriteria(Func<T, bool> selector)
        {
            var result = await Task.Run(() =>
            {
                using (var context = CreateReadOnlyContext())
                {
                    var table = context.GetTable<T>();
                    var o = table.Where(selector).ToList();
                    return o;
                }
            });
            return result;
        }

        public async Task Insert(T obj)
        {
            if (ValidationEngine != null)
                ValidationEngine.Validate(obj);

            await Task.Run(() =>
            {
                using (var context = CreateContext())
                {
                    var table = context.GetTable<T>();
                    table.InsertOnSubmit(obj);
                    context.SubmitChanges();
                }
            });
        }
        
        public async Task<TId> Insert<TId>(T obj, Func<T, TId> idSelector)
        {
            if (ValidationEngine != null)
                ValidationEngine.Validate(obj);

            var result = await Task.Run(() =>
            {
                using (var context = CreateContext())
                {
                    var table = context.GetTable<T>();
                    table.InsertOnSubmit(obj);
                    context.SubmitChanges();
                    var o = idSelector(obj);
                    return o;
                }
            });
            return result;
        }

        public async Task Update(T update)
        {
            if (ValidationEngine != null)
                ValidationEngine.Validate(update);

            await Task.Run(() =>
            {
                using (var context = CreateContext())
                {
                    var table = context.GetTable<T>();
                    table.Attach(update, true);
                    context.SubmitChanges();
                }
            });
        }

        public async Task Delete(T obj)
        {
            await Task.Run(() =>
            {
                using (var context = CreateContext())
                {
                    var table = context.GetTable<T>();
                    table.Attach(obj, true);
                    table.DeleteOnSubmit(obj);
                    context.SubmitChanges();
                }
            });
        }
    }
}
