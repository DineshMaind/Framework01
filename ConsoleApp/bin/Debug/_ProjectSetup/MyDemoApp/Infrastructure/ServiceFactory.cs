using MyDemoApp.Core;
using System.Web.Mvc;

namespace MyDemoApp.Infrastructure
{
    public class ServiceFactory : IServiceFactory
    {
        private static readonly ServiceFactory instance = new ServiceFactory();

        static ServiceFactory()
        {
        }

        private ServiceFactory()
        {
        }

        public static IServiceFactory Instance { get { return instance; } }

        public IRepository GetDatabaseObject()
        {
            return DependencyResolver.Current.GetService<IRepository>();
        }
    }
}