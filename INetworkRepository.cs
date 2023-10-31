using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface INetworkRepository
    {
        Task<NetworkView> GetNetworkByFiscalNumberRoot(NetworkSearchParameters parameters);

        Task<NetworkView[]> GetNetworksByName(NetworkSearchParameters parameters);

        Task<int> SaveNetworks(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);

        Task<int> SaveNetworkDiscounts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
    }
}
