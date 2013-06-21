using Ninject;

namespace Weave.Identity.Service.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var connectionString =
"Server=tcp:rp8dpm2k1x.database.windows.net,1433;Database=weaveuserfeed_db;User ID=aemami99@rp8dpm2k1x;Password=rzarecta99!;Trusted_Connection=False;Encrypt=False;Connection Timeout=30;";

            //Bind<SqlStoredProcClient>().ToMethod(_ => new SqlStoredProcClient(connectionString));
        }
    }
}
