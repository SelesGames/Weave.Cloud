using Common.Validation;
using System;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Data.Linq
{
    public class TransactionalLinqDbClient : LinqDbClient, ITransactionalDatabaseClient, IDisposable
    {
        DataContext activeContext;

        public int CommandTimeout
        {
            get { return activeContext.CommandTimeout; }
            set { activeContext.CommandTimeout = value; }
        }

        public TransactionalLinqDbClient(SqlServerCredentials credentials) : 
            base(credentials) 
        {
            BeginUnitOfWork();
        }

        public ValidationEngine ValidationEngine { get; set; }

        public Task<IQueryable<T>> Get<T>()
             where T : class
        {
            var table = activeContext.GetTable<T>();
            var o = table;
            return Task.FromResult<IQueryable<T>>(o);
        }

        public void Insert<T>(T obj)
             where T : class
        {
            if (ValidationEngine != null)
                ValidationEngine.Validate(obj);

            var table = activeContext.GetTable<T>();
            table.InsertOnSubmit(obj);
        }

        public void Update<T>(T update)
             where T : class
        {
            if (ValidationEngine != null)
                ValidationEngine.Validate(update);

            var table = activeContext.GetTable<T>();
            table.Attach(update, true);
        }

        public void Delete<T>(T obj)
             where T : class
        {
            var table = activeContext.GetTable<T>();
            table.DeleteOnSubmit(obj);
        }

        void BeginUnitOfWork()
        {
            activeContext = CreateContext();
        }

        public void Dispose()
        {
            if (activeContext != null)
                activeContext.Dispose();
        }

        public Task SubmitChanges()
        {
            activeContext.SubmitChanges();
            return Task.FromResult<object>(null);
        }
    }
}
