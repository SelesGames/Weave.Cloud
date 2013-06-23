using Common.Data;
using Common.Data.Linq;
using Ninject;

namespace Weave.Identity.Service.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        readonly string CONNECTION_STRING =
"Server=tcp:eb0l5hjvzm.database.windows.net,1433;Database=weaveaccount_db;User ID=aemami99@eb0l5hjvzm;Password=rzarecta99!;Trusted_Connection=False;Encrypt=False;Connection Timeout=30;";
        
        protected override void AddComponents()
        {
            base.AddComponents();
            Bind<ITransactionalDatabaseClient>().ToMethod(_ => CreateDatabaseClient()).InTransientScope();
        }

        ITransactionalDatabaseClient CreateDatabaseClient()
        {
            var credentials = new SqlServerCredentials
            {
                ConnectionString = CONNECTION_STRING,
            };
            return new TransactionalLinqDbClient(credentials);
        }
    }
}
