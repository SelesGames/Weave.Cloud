﻿using Common.Azure;
using Common.Data;
using Common.Data.Linq;
using Newtonsoft.Json;
using Ninject;
using SelesGames.Common;
using Weave.AccountManagement.DTOs;

namespace Weave.AccountManagement.WebRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var blobClient = new AzureJsonDotNetBlobClient<UserInfo>(
                "weave",
                "uudFrra70qkI64bifaI2Rrm37CZ1HkzaBQrLMyw6U/hmzNDZehXeo9DdUv7BCpuZY4N2q/CNpNwYxW2fa218xA==",
                "userinfo",
                false) { SerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented }, UseGzipOnUpload = true };

            var userManager = new UserManager(blobClient);

            Bind<UserManager>().ToConstant(userManager);

            var connectionString =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

            var credentials = new SqlServerCredentials { ConnectionString = connectionString };
            Bind<SqlServerCredentials>().ToConstant(credentials);
            Bind<IProvider<ITransactionalDatabaseClient>>()
                .ToMethod(_ => new DelegateProvider<ITransactionalDatabaseClient>(() => new TransactionalLinqDbClient(credentials)))
                .InSingletonScope();
        }
    }
}