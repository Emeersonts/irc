using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence.Api
{
    public interface ICouponRepository
    {
        Task<Coupon[]> SearchCoupon(CouponSearchParameters parameters);
        Task RegisterCampaignCoupon(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
        Task<Coupon> GetDraftCouponByCampaign(CouponSearchParameters parameters);
    }
}
