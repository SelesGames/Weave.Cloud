using System.Web.Http.Dependencies;

namespace Ninject.WebApi
{
    public class NinjectResolver : NinjectScope, IDependencyResolver
    {
        IKernel kernel;

        public NinjectResolver(IKernel kernel)
            : base(kernel)
        {
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectScope(kernel.BeginBlock());
        }
    }
}
