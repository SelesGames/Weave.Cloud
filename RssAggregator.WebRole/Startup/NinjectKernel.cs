using Common.Data;
using Common.Data.Linq;
using Ninject;
using SelesGames.Common;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();
        //}
        //public NinjectKernel()
        //{
            var connectionString =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

            Bind<SqlServerCredentials>()
                .ToConstant(new SqlServerCredentials { ConnectionString = connectionString })
                .InSingletonScope();

    //        var azureCredentials = AzureStorageCredentials.Create(
    //"eentertainmentcms", "Y96IzFM79dM1WxQn6FwOEdv5DvDHWZOBsEuOiDbFN7YKNf5eeWzk9KNltroyUMmafCgovpSw0q66oTCpSFNoJA==", useHttps: false);

    //        Bind<AzureStorageCredentials>().ToConstant(azureCredentials).InSingletonScope();

    //        Bind<ValidationEngine>().To<CMSValidationEngine>().InThreadScope();

            Bind<IProvider<ITransactionalDatabaseClient>>().ToMethod(_ =>
            {
                return DelegateProvider.Create(() =>
                {
                    var client = this.Get<TransactionalLinqDbClient>();
                    client.CommandTimeout = 30000;
                    return client;
                });
            });

            Bind<SqlStoredProcClient>().ToMethod(_ => new SqlStoredProcClient(connectionString));
        }
    }
}
