using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace Ninject.WebApi
{
    public class NinjectScope : IDependencyScope
    {
        readonly IKernel kernel;

        public NinjectScope(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public void Dispose() { }
    }
}
