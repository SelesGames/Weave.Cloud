﻿using Common.Azure.ServiceBus;
using Common.Caching;
using Common.Data;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using RssAggregator.IconCaching;
using SelesGames.Common.Hashing;
using System;
using System.Threading.Tasks;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();


            var connectionString =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

            var serviceBusCredentials = new ServiceBusCredentials
            {
                Namespace = "weave-interop",
                IssuerName = "owner",
                IssuerKey = "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=",
            };
            var clientFactory = new ClientFactory(serviceBusCredentials);

            var roleId = RoleEnvironment.CurrentRoleInstance.UpdateDomain;
            var roleInstanceHash = CryptoHelper
                .ComputeHashUsedByMobilizer(RoleEnvironment.CurrentRoleInstance.Role.Name)
                .ToString("N")
                .Substring(0, 6);

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd_HH.mm");

            var subName = RoleEnvironment.IsEmulated ?
                string.Format("{0}_{1}_emu_{2}", roleInstanceHash, now, Guid.NewGuid().ToString("N").Substring(0, 6))
                :
                string.Format("{0}_{1}_{2}", roleInstanceHash, now, Guid.NewGuid().ToString("N").Substring(0, 6));

            var subscriptionConnector = new SubscriptionConnector(clientFactory, "FeedUpdatedTopic", subName);

            Bind<ServiceBusCredentials>().ToConstant(serviceBusCredentials);
            Bind<ClientFactory>().ToConstant(clientFactory);
            Bind<SubscriptionConnector>().ToConstant(subscriptionConnector);

            Bind<SqlStoredProcClient>().ToMethod(_ => new SqlStoredProcClient(connectionString));


            var hfCache = new HighFrequencyFeedIconUrlCache();
            hfCache.BeginListeningToResourceChanges();

            var caches = new IExtendedCache<string, Task<string>>[] 
            {
                hfCache,
                new IconUrlAzureDataCache(),
                new DynamicIconUrlCache()
            };

            var nLevelCache = new NLevelIconUrlCache(caches);

            Bind<NLevelIconUrlCache>().ToConstant(nLevelCache).InSingletonScope();
        }
    }
}
