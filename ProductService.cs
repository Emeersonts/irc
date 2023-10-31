using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class ProductService: IProductService
    {
        private readonly IProductRepository repository;

        public ProductService(IProductRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Brand[]> GetBrands(ProductSearchParameters parameters)
        {
            return await repository.GetProductBrands(parameters);
        }

        public async Task<ProductItem[]> GetProductsByCompany(ProductSearchParameters parameters)
        {
            return await this.repository.GetProductsByCompany(parameters);
        }
    }
}
