using Common.Validation;
using System;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Data.Linq
{
    public class TransactionalLinqDbClient : LinqDbClient, ITransactionalDatabaseClient
    {
        DataContext activeContext;
        bool isActiveContextInUse = false;

        public TransactionalLinqDbClient(SqlServerCredentials credentials) : base(credentials) { }

        public ValidationEngine ValidationEngine { get; set; }

        public Task<IQueryable<T>> Get<T>()
             where T : class
        {
            EnsureActiveContext();

            var table = activeContext.GetTable<T>();
            var o = table;
            return Task.FromResult<IQueryable<T>>(o);
        }

        public async Task Insert<T>(T obj)
             where T : class
        {
            EnsureActiveContext();

            if (ValidationEngine != null)
                ValidationEngine.Validate(obj);

            await Task.Run(() =>
            {
                var table = activeContext.GetTable<T>();
                table.InsertOnSubmit(obj);
            });
        }

        public async Task Update<T>(T update)
             where T : class
        {
            EnsureActiveContext();

            if (ValidationEngine != null)
                ValidationEngine.Validate(update);

            await Task.Run(() =>
            {
                var table = activeContext.GetTable<T>();
                table.Attach(update, true);
            });
        }

        public async Task Delete<T>(T obj)
             where T : class
        {
            EnsureActiveContext();

            await Task.Run(() =>
            {
                var table = activeContext.GetTable<T>();
                table.DeleteOnSubmit(obj);
            });
        }

        public IUnitOfWork BeginUnitOfWork()
        {
            if (isActiveContextInUse)
                throw new InvalidOperationException("You can only call BeginUnitOfWork after disposing of the previous IUnitOfWork!");

            isActiveContextInUse = true;
            activeContext = CreateContext();
            return new DataContextUnitOfWorkAdapter(activeContext, () => isActiveContextInUse = false);
        }

        void EnsureActiveContext()
        {
            if (!isActiveContextInUse)
                throw new InvalidOperationException("An active Unit of Work is required!");
        }

        public void Dispose()
        {
            if (activeContext != null)
                activeContext.Dispose();
        }
    }

    internal class DataContextUnitOfWorkAdapter : IUnitOfWork
    {
        DataContext context;
        Action onDispose;

        public DataContextUnitOfWorkAdapter(DataContext context, Action onDispose)
        {
            this.context = context;
            this.onDispose = onDispose;
        }

        public Task SubmitChanges()
        {
            context.SubmitChanges();
            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
            context.Dispose();
            onDispose();
        }
    }
}
