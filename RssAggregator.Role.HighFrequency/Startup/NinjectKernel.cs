using Common.Azure.ServiceBus;
using Common.Data;
using Common.Data.Linq;
using Ninject;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using Weave.RssAggregator.HighFrequency;
using Weave.RssAggregator.Parsing;

namespace RssAggregator.Role.HighFrequency
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

            Bind<ServiceBusCredentials>().ToConstant(new ServiceBusCredentials
                {
                    Namespace = "weave-interop",
                    IssuerName = "owner",
                    IssuerKey = "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=",
                });

            Bind<TopicConnector>().ToMethod(_ => new TopicConnector(this.Get<ServiceBusCredentials>(), "FeedUpdatedTopic"))
                .WhenInjectedExactlyInto<Weave.RssAggregator.HighFrequency.ServiceBusUpdater>()
                .InSingletonScope();

            Bind<SequentialProcessor>().ToMethod(_ => new SequentialProcessor(
                new ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>[]
                {
                    this.Get<SqlUpdater>(),
                    this.Get<ServiceBusUpdater>(),
                }))
                .InSingletonScope();
        }
    }
}