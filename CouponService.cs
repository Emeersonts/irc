using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class CouponService: ICouponService
    {
        private readonly ICouponRepository repository;

        public CouponService(ICouponRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Coupon[]> GetCouponsRanges(CouponSearchParameters parameters)
        {
            return await repository.SearchCoupon(parameters);
        }
    }
}
