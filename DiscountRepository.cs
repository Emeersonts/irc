using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IDataContext dataContext;

        public DiscountRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<Discount[]> FindAllDiscount()
        {
            var discountList = new List<Discount>();

            var query = @"SELECT Code,Name, Value FROM [Discount] ORDER BY NAME";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var discountlistentity = await connectionContext.QueryAsync<DiscountEntity>(query);

                foreach (var discount in discountlistentity)
                {
                    discountList.Add(new Discount(discount.Code, discount.Name, discount.Value));
                }

                return discountList.ToArray();
            }
        }

    }
}
