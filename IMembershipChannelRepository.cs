using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IMembershipChannelRepository
    {
        Task<int> SaveChannels(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
    }
}
