using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface ICouponService
    {
        Task<Coupon[]> GetCouponsRanges(CouponSearchParameters parameters);
    }
}
