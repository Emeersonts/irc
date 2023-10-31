using IDP.Authorizer;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class CompanyProgramRepository : ICompanyProgramRepository
    {
        ICampaignConfigurationFactory campaignConfigurationFactory;

        public CompanyProgramRepository(ICampaignConfigurationFactory campaignConfigurationFactory)
        {
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async Task<ProgramView[]> GetProgramsByCompany(int acquirerCode)
        {
            var programs = new List<ProgramView>();
            var programDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = programDataContext.AcquireConnection())
            {
                var sql = @"SELECT 
				                    [Acct_Nr] [Id]
                                    ,RTRIM([Acct_Name]) [Name]
                               FROM [TRNCENTRE_ADV_V7].[dbo].[Str_Acct_Parms]
                               WHERE 
	                                    Rec_St = 'A' 
					                    AND Acquirer_Code = @AcquirerCode";
                var result = await connectionContext.QueryAsync<ProgramView>(sql, new {AcquirerCode = acquirerCode });
                if(result != null && result.Any())
                {
                    return result.ToArray();
                }

            }

            return programs.ToArray();
        }


        public async Task<ProgramView[]> GetProgramsById(CompanyProgramSearchParameters parameters)
        {
            var programs = new List<ProgramView>();
            var programDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using (var connectionContext = programDataContext.AcquireConnection())
            {
                var sql = @"SELECT 
				                [Acct_Nr] [Id]
                                ,RTRIM([Acct_Name]) [Name]
                            FROM [TRNCENTRE_ADV_V7].[dbo].[Str_Acct_Parms]
                            WHERE 
	                            Rec_St = 'A' 
					            AND Acquirer_Code = @AcquirerCode
					AND (Acct_Nr IN (
					 (SELECT 
							vl_camp 
					  FROM [TRNCENTRE_ADV_V7].[dbo].[fn_split] (@PreviousCodes,',')
					  )) OR Acct_Nr = TRY_PARSE(@PreviousCodes as int)
					 )";
                var result = await connectionContext.QueryAsync<ProgramView>(sql, new { AcquirerCode = parameters.AcquirerCode,
                PreviousCodes = parameters.ProgramIds});
                if (result != null && result.Any())
                {
                    return result.ToArray();
                }

            }

            return programs.ToArray();
        }
    }
}
