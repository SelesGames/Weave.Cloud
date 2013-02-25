using Common.Azure.ServiceBus;
using Common.Data;
using Common.Data.Linq;
using Ninject;
using SelesGames.Common;
using Weave.RssAggregator.LowFrequency;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();


            var connectionString =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

            Bind<SqlServerCredentials>()
                .ToConstant(new SqlServerCredentials { ConnectionString = connectionString })
                .InSingletonScope();

            var serviceBusCredentials = new ServiceBusCredentials
            {
                Namespace = "weave-interop",
                IssuerName = "owner",
                IssuerKey = "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=",
            };
            var clientFactory = new ClientFactory(serviceBusCredentials);
            var subscriptionConnector = new SubscriptionConnector(clientFactory, "FeedUpdatedTopic");

            Bind<ServiceBusCredentials>().ToConstant(serviceBusCredentials);
            Bind<ClientFactory>().ToConstant(clientFactory);
            Bind<SubscriptionConnector>().ToConstant(subscriptionConnector);

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
