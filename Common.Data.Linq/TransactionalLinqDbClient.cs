using Common.Validation;
using System;
using System.Data.Linq;
using System.Linq;

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

        public IQueryable<T> Get<T>()
             where T : class
        {
            return activeContext.GetTable<T>();
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

        public void SubmitChanges()
        {
            activeContext.SubmitChanges();
        }
    }
}
