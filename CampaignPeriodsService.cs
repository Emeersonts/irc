using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class CampaignPeriodsService : ICampaignPeriodsService
    {

        private readonly ICampaignPeriodsRepository campaignPeriodsrepository;

        public CampaignPeriodsService(ICampaignPeriodsRepository campaignPeriodsrepository)
        {
            this.campaignPeriodsrepository = campaignPeriodsrepository;
        }
        
        public async Task<CampaignPeriods[]> FindAllCampaignPeriods()
        {
            return await campaignPeriodsrepository.FindAllCampaignPeriods();
        }

    }
}
