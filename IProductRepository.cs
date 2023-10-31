using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IProductRepository
    {
        Task<ProductItem[]> GetProductsByCompany(ProductSearchParameters parameters);
        Task SaveProducts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
        Task<short> GetProductFullBox(ProductSearchParameters parameters);
        Task<Brand[]> GetProductBrands(ProductSearchParameters parameters);
        Task<Brand> GetProductBrandByProduct(string ean);
        Task<string> GetProductDescriptionByEAN(int acquirerCode, string ean);
        Task<Product[]> GetProductsByCampaign(Campaign campaign);
        Task<Brand> GetBrandById(int id);
    }
}
