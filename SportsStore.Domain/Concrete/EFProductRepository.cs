using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Abstract;

namespace SportsStore.Domain.Concrete
{
    public class EFProductRepository: IProductRepository
    {
        private EFDbContext context = new EFDbContext();

        public IEnumerable<Product> Products
        {
            get
            {
                return context.Products;
            }
        }
    }
}
