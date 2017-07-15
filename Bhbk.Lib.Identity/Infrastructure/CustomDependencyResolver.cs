using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Unity;
using Unity.Exceptions;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class CustomDependencyResolver : IDependencyResolver
    {
        protected IUnityContainer _container;

        public CustomDependencyResolver(IUnityContainer container)
        {
            if (container == null)
                throw new ArgumentNullException();

            _container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            var child = _container.CreateChildContainer();
            return new CustomDependencyResolver(child);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
