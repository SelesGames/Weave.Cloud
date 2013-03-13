using Common.Azure.ServiceBus;
using Common.Azure.ServiceBus.Reactive;
using Common.Data;
using Common.Data.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using SelesGames.Common;
using SelesGames.Common.Hashing;
using System;

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

            var roleId = RoleEnvironment.CurrentRoleInstance.UpdateDomain;
            var roleInstanceHash = CryptoHelper.ComputeHashUsedByMobilizer(RoleEnvironment.CurrentRoleInstance.Role.Name).ToString();
            roleInstanceHash = roleInstanceHash.Replace("-", null).Substring(0, 16);

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd_HH.mm");

            var subName = RoleEnvironment.IsEmulated ?
                string.Format("Role_{0}_{1}_emulator_{2}", roleId, now, roleInstanceHash)
                :
                string.Format("Role_{0}_{1}_{2}", roleId, now, roleInstanceHash);

            var subscriptionConnector = new SubscriptionConnector(clientFactory, "FeedUpdatedTopic", subName);

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
