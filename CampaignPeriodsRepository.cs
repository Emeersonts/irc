using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class CampaignPeriodsRepository : ICampaignPeriodsRepository
    {

        private readonly IDataContext dataContext;

        public CampaignPeriodsRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }


        public async Task<CampaignPeriods[]> FindAllCampaignPeriods()
        {
            var campaignPeriodsList = new List<CampaignPeriods>();

            var query = @"SELECT Code,Name, Value FROM [CampaignPeriods] ORDER BY NAME";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var campaignPeriodsListentity = await connectionContext.QueryAsync<CampaignPeriodsEntity>(query);

                foreach (var campaignperiods in campaignPeriodsListentity)
                {
                    campaignPeriodsList.Add(new CampaignPeriods(campaignperiods.Code, campaignperiods.Name, campaignperiods.Value));
                }

                return campaignPeriodsList.ToArray();
            }

        }

    }
}
