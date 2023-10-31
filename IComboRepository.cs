using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IComboRepository
    {
        Task<int> SaveCombos(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);

        Task<int> SaveBrandCombos(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
    }
}
