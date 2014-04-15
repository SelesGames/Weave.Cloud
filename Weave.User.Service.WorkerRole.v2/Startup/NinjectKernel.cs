using Ninject;
using Weave.Article.Service.Contracts;
using Weave.User.BusinessObjects.v2.Repositories;

namespace Weave.User.Service.WorkerRole.v2.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var storageAccount = "weaveuser2";
            var storageKey =
"JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==";

            var repo = new UserInfoRepository(storageAccount, storageKey);

            Bind<IUserInfoRepository>().ToConstant(repo).InSingletonScope();
            Bind<IWeaveArticleService>().To<Article.Service.Client.ServiceClient>().InSingletonScope();
        }
    }
}
