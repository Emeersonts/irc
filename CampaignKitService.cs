using BackOffice.Authorizer.Management.Core.Contracts;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class CampaignKitService : ICampaignKitService
    {
        private readonly IKitRepository repository;

        public CampaignKitService(IKitRepository repository)
        {
            this.repository = repository;
        }

        public async Task<CampaignKitDTO[]> GetKitsByCampaign(CampaignKitSearchParameters parameters)
        {
            return await this.repository.GetKitsByCampaign(parameters);
        }
    }
}
