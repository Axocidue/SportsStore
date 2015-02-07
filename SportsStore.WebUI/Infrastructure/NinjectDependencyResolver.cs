using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Moq;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Concrete;
using System.Configuration;



namespace SportsStore.WebUI.Infrastructure
{
    public class NinjectDependencyResolver: IDependencyResolver
    {
        private IKernel kernel;
        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }
        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }
        private void AddBindings()
        {
            // put bindings here


            //var mock = new Mock<IProductRepository>();
            //mock.Setup(m => m.Products).Returns(
            //    new List<Product>
            //    {
            //        new Product { Name = "Xperia Z3", Price = 5499M},
            //        new Product {Name = "iPhone 6Plus", Price = 5999M},
            //        new Product {Name = "Lumia 1020", Price = 3999M},
            //        new Product {Name = "Galaxy Notes 4", Price = 3499M}
            //    });

            //kernel.Bind<IProductRepository>().ToConstant(mock.Object);

            kernel.Bind<IProductRepository>().To<EFProductRepository>();

            var emailSettings = new EmailSettings
            {
                WriteAsFile = bool.Parse(ConfigurationManager.AppSettings["Email.WriteAsFile"] ?? "false")
            };

            kernel.Bind<IOrderProcessor>().To<EmailOrderProcessor>()
                .WithConstructorArgument("settings", emailSettings);

        }
    }
}