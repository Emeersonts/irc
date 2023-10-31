using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IProgressiveDiscountRepository
    {
        Task<int> SaveProgressiveDiscountsByThreshold(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
        Task<int> SaveMonthlyProgressiveDiscounts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
        Task<int> SaveUnitaryProgressiveDiscounts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
    }
}
