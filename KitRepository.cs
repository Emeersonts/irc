using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class KitRepository : IKitRepository
    {
        private readonly IDataContext dataContext;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;

        public KitRepository(
            ICampaignConfigurationFactory campaignConfigurationFactory,
            IDataContext dataContext)
        {
            this.campaignConfigurationFactory = campaignConfigurationFactory;
            this.dataContext = dataContext;
        }

        public async Task<CampaignKitDTO[]> GetKitsByCampaign(CampaignKitSearchParameters parameters)
        {
            var kits = new List<CampaignKitDTO>();

            using (var connecitonContext = dataContext.AcquireConnection())
            {
                var sql = @"SELECT 
                                   [Rules_Code] as [CampaignId]
                                  ,[Rules_BeginDate] as [BeginDate]
                                  ,[Prd_StdCode] as [EAN]
                                  ,[Prd_NewKitQty] as [Quantity]
                                  ,[Prd_NewKitDesc] as [Discount]
                                  ,[Rec_St] as [Status]
                              FROM [dbo].[Rules_PrdNewKit]
                              WHERE Rules_Code = @CampaignId AND
		                            Rules_BeginDate = @CampaignBeginDate
                              ORDER BY Rules_BeginDate DESC";

                kits = (await connecitonContext.QueryAsync<CampaignKitDTO>(sql, new
                {
                    CampaignId = parameters.CampaignId,
                    CampaignBeginDate = parameters.CampaignBeginDate
                })).ToList();

            }

            return kits.ToArray();

        }

        public async Task<int> SaveKits(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"INSERT INTO [dbo].[Rules_PrdNewKit]
                           ([Rules_Code]
                           ,[Rules_BeginDate]
                           ,[Prd_StdCode]
                           ,[Prd_NewKitQty]
                           ,[Prd_NewKitDesc]
                           ,[Rec_St]
                           ,[Rec_StInfo]
                           ,[Rec_InsTime]
                           ,[Rec_InsUser]
                           ,[Rec_UpdTime]
                           ,[Rec_UpdUser]
                           ,[Rec_SetTime]
                           ,[Rec_SetUser])
                           VALUES
                           (
		                   @Rules_Code
                           ,@Rules_BeginDate
                           ,@Prd_StdCode
                           ,@Prd_NewKitQty
                           ,@Prd_NewKitDesc
                           ,@Rec_St
                           ,@Rec_StInfo
                           ,@Rec_InsTime
                           ,@Rec_InsUser
                           ,@Rec_UpdTime
                           ,@Rec_UpdUser
                           ,@Rec_SetTime
                           ,@Rec_SetUser
		                  )
                           SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM Rules_PrdNewKit where Rules_Code = @Rules_Code

                                     AND Rules_BeginDate = @Rules_BeginDate", new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.Kits != null && campaign.Kits.Count() > 0)
                {

                    foreach (var kit in campaign.Kits)
                    {
                        Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirKits");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("KTQ", kit.Quantity);
                        messageParameters.Add("KTD", kit.Discount);
                        messageParameters.Add("EAN", kit.Product.EAN);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new CampaignKitEntity()
                            {
                                Prd_NewKitDesc = kit.Discount,
                                Prd_NewKitQty = kit.Quantity,
                                Prd_StdCode = Convert.ToDecimal(kit.Product.EAN),
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_SetTime = DateTime.Now,
                                Rec_SetUser = string.Empty,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,

                            })).ToList().First();
                        }
                    }
                }

            }

            return recordsAffected;
        }
    }
}
