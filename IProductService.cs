using BackOffice.Authorizer.Management.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface IProductService
    {
        Task<ProductItem[]> GetProductsByCompany(ProductSearchParameters parameters);

        Task<Brand[]> GetBrands(ProductSearchParameters parameters);
    }
}
