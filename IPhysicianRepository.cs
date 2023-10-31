using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IPhysicianRepository
    {
        Task<int> SavePhysicians(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
    }
}
