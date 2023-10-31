using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class DiscountService : IDiscountService
    {
        
        private readonly IDiscountRepository discountreposotory;

        public DiscountService(IDiscountRepository discountrepository)
        {
            this.discountreposotory = discountrepository;
        }

        public async Task<Discount[]> FindAllDiscount()
        {
            return await discountreposotory.FindAllDiscount();
        }

    }
}
