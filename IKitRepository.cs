using BackOffice.Authorizer.Management.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IKitRepository
    {
        Task<CampaignKitDTO[]> GetKitsByCampaign(CampaignKitSearchParameters parameters);
        Task<int> SaveKits(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
    }
}
