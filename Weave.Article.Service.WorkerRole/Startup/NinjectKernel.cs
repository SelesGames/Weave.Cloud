using Common.Data;
using Ninject;

namespace Weave.Article.Service.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var connectionString =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

            Bind<SqlStoredProcClient>().ToMethod(_ => new SqlStoredProcClient(connectionString));
        }
    }
}
