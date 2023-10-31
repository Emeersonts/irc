using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class NetworkRepository : INetworkRepository
    {
        private readonly IDataContext dataContext;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;

        public NetworkRepository(
            ICampaignConfigurationFactory campaignConfigurationFactory,
            IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async Task<NetworkView> SearchNetworks(NetworkSearchParameters parameters)
        {

            var query =
            @"SELECT TOP 1 RTRIM(company.MsfCo_FederalName) [CFN],SUBSTRING(company.MsfCo_FederalCode,1,8) [FNR] FROM 
			[TRNCENTRE_ADV_V7].dbo.[Str_CoMsFile] company INNER JOIN
			[TRNCENTRE_ADV_V7].dbo.[Str_Estab_Acquirer] info ON
			info.Estab_FederalCode = company.MsfCo_FederalCode AND info.Rec_St = 'A'
			WHERE info.Acquirer_Code = @AcquirerCode
            AND SUBSTRING(company.MsfCo_FederalCode,1,8) = @FNR";

            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var network = (await connectionContext.QueryAsync<NetworkView>(query, new { AcquirerCode = parameters.AcquirerCode, FNR = parameters.FiscalNumberRoot })).FirstOrDefault();

                return network;
            }
        }


        public async Task<NetworkView> GetNetworkByFiscalNumberRoot(NetworkSearchParameters parameters)
        {

            var query =
            @"SELECT TOP 1 RTRIM(company.MsfCo_FederalName) [Name],SUBSTRING(company.MsfCo_FederalCode,1,8) [FiscalNumberRoot] FROM  
			[TRNCENTRE_ADV_V7].dbo.[Str_CoMsFile] company INNER JOIN
			[TRNCENTRE_ADV_V7].dbo.[Str_Estab_Acquirer] info ON 
			info.Estab_FederalCode = company.MsfCo_FederalCode AND info.Rec_St = 'A' 
			WHERE info.Acquirer_Code = @ACQ
            AND SUBSTRING(company.MsfCo_FederalCode,1,8) = @FNR";

            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
           
            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var network = (await connectionContext.QueryAsync<NetworkView>(query, new { ACQ = parameters.AcquirerCode, FNR = parameters.FiscalNumberRoot })).ToArray();
                
                var gg = new NetworkView();
                foreach (var item in network)
                {
                    gg.FiscalNumberRoot = item.FiscalNumberRoot;
                    gg.Name = item.Name;
                }

                return gg;
            }
        }

        public async Task<int> SaveNetworks(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"INSERT INTO [dbo].[Rules_MsFile_Specific]
                                   ([Acquirer_Code]
                                   ,[Rules_Code]
                                   ,[Rules_BeginDate]
                                   ,[Estab_Chain]
                                   ,[SystemType]
                                   ,[Rec_St]
                                   ,[Rec_StInfo]
                                   ,[Rec_InsTime]
                                   ,[Rec_InsUser]
                                   ,[Rec_UpdTime]
                                   ,[Rec_UpdUser])
                             VALUES
                                   (@Acquirer_Code
                                   ,@Rules_Code
                                   ,@Rules_BeginDate
                                   ,@Estab_Chain
                                   ,@SystemType
                                   ,@Rec_St
                                   ,@Rec_StInfo
                                   ,@Rec_InsTime
                                   ,@Rec_InsUser
                                   ,@Rec_UpdTime
                                   ,@Rec_UpdUser)
                             SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"delete from Rules_MsFile_Specific where Rules_Code = Rules_Code
			                     AND Rules_BeginDate = Rules_BeginDate AND  RTRIM(Estab_Chain) <> ''", new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.Networks != null && campaign.Networks.Count() > 0)
                {
                    foreach (var network in campaign.Networks)
                    {
                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new RulesSpecificEntity()
                            {

                                Estab_Chain = network.FiscalNumberRoot,
                                Acquirer_Code = campaign.AcquirerCode,
                                SystemType = string.Empty,
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,

                            })).ToList().First();
                        }
                        Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirRedes");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("FNR", network.FiscalNumberRoot);
                        messageParameters.Add("CHR", new string(' ', 15));
                        messageParameters.Add("ACQ", campaign.AcquirerCode);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
                    }
                }

            }

            return recordsAffected;
        }

        public async Task<int> SaveNetworkDiscounts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"INSERT INTO [dbo].[Rules_PrdFlag]
                                   ([Rules_Code]
                                   ,[Rules_BeginDate]
                                   ,[Rules_FlagBeginDate]
                                   ,[Rules_FlagEndDate]
                                   ,[Prd_StdCode]
                                   ,[MsfCo_FlagGroup]
                                   ,[State]
                                   ,[Prc_DescAd]
                                   ,[Rec_St]
                                   ,[Rec_StInfo]
                                   ,[Rec_InsTime]
                                   ,[Rec_InsUser]
                                   ,[Rec_UpdTime]
                                   ,[Rec_UpdUser])
                             VALUES
                                   (
		                            @Rules_Code
                                   ,@Rules_BeginDate
                                   ,@Rules_FlagBeginDate
                                   ,@Rules_FlagEndDate
                                   ,@Prd_StdCode
                                   ,@MsfCo_FlagGroup
                                   ,@State
                                   ,@Prc_DescAd
                                   ,@Rec_St
                                   ,@Rec_StInfo
                                   ,@Rec_InsTime
                                   ,@Rec_InsUser
                                   ,@Rec_UpdTime
                                   ,@Rec_UpdUser
		                           )
                               SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"delete from [dbo].[Rules_PrdFlag] where Rules_Code = Rules_Code
			                     AND Rules_BeginDate = Rules_BeginDate", new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.AdditionalNetworkDiscounts != null && campaign.AdditionalNetworkDiscounts.Count() > 0)
                {
                    foreach (var discount in campaign.AdditionalNetworkDiscounts)
                    {
                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new AdditionalNetworkDiscountEntity()
                            {
                                MsfCo_FlagGroup = discount.FiscalNumberRoot,
                                Prc_DescAd = discount.Discount,
                                Prd_StdCode = Convert.ToDecimal(discount.Product.EAN),
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rules_FlagBeginDate = campaign.BeginDate,
                                Rules_FlagEndDate = campaign.EndDate,
                                State = discount.State.Abbreviation,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id,
                            })).ToList().First();
                        }

                        Dictionary<string, object> messsageParameters = new Dictionary<string, object>();
                        messsageParameters.Add("FUN", "IRC_InserirDescRede");
                        messsageParameters.Add("RLC", campaign.Id);
                        messsageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messsageParameters.Add("RED",campaign.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messsageParameters.Add("USC", campaign.RecordInsertionUser);
                        messsageParameters.Add("FNR", discount.FiscalNumberRoot);
                        messsageParameters.Add("EAN", discount.Product.EAN);
                        messsageParameters.Add("STC", discount.State.Abbreviation);
                        messsageParameters.Add("PRD", discount.Discount);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messsageParameters));
                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                    }
                }

                return recordsAffected;
            }
        }

        public async Task<NetworkView[]> GetNetworksByName(NetworkSearchParameters parameters)
        {
            var networkDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            var networks = new List<NetworkView>();
            using (var connectionContext = networkDataContext.AcquireConnection())
            {
                var sql = @"SELECT  RTRIM(company.MsfCo_FederalName) [Name]
                                    ,SUBSTRING(company.MsfCo_FederalCode,1,8) [FiscalNumberRoot] FROM 
			                [TRNCENTRE_ADV_V7].[dbo].[Str_CoMsFile] company INNER JOIN
			                [TRNCENTRE_ADV_V7].[dbo].[Str_Estab_Acquirer] info ON
			                info.Estab_FederalCode = company.MsfCo_FederalCode AND 
			                info.Rec_St = 'A'
			                WHERE info.Acquirer_Code = @AcquirerCode AND
			                company.MsfCo_Name LIKE  @Name + '%'";
                var result = await connectionContext.QueryAsync<NetworkView>(sql, new { 
                AcquirerCode = parameters.AcquirerCode, Name = parameters.NetworkName });

                if(result != null && result.Any())
                {
                    return result.ToArray();
                }
            }

            return networks.ToArray();
        }
    }
}
