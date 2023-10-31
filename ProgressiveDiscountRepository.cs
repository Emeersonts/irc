using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class ProgressiveDiscountRepository : IProgressiveDiscountRepository
    {
        private readonly IDataContext dataContext;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;

        public ProgressiveDiscountRepository(
            IDataContext dataContext,
            ICampaignConfigurationFactory campaignConfigurationFactory)
        {
            this.dataContext = dataContext;
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async Task<int> SaveMonthlyProgressiveDiscounts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;
            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync("DELETE FROM [dbo].[Rules_MultiPrd_Month] WHERE Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate", new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });
                if (campaign.MonthlyDiscounts != null && campaign.Products != null && campaign.MonthlyDiscounts.Count() > 0)
                {


                    foreach (var discount in campaign.MonthlyDiscounts)
                    {
                        string sql = @"INSERT INTO [dbo].[Rules_MultiPrd_Month]
                                        ([Rules_Code]
                                        ,[Rules_BeginDate]
                                        ,[Prd_DescQtyBegin]
                                        ,[Prd_DescQtyEnd]
                                        ,[Prd_RestartQty]
                                        ,[Prd_PeriodMax]
                                        ,[Prd_DescUpdId]
                                        ,[Prd_Desc]
                                        ,[Prd_Desc_Penalty]
                                        ,[Mileage]
                                        ,[Prd_RemoveQty]
                                        ,[Prd_RestartCycle]
                                        ,[Prd_DescInfo]
                                        ,[Rules_CodeOut]
                                        ,[Rec_St]
                                        ,[Rec_StInfo]
                                        ,[Rec_InsTime]
                                        ,[Rec_InsUser]
                                        ,[Rec_UpdTime]
                                        ,[Rec_UpdUser])
                                    VALUES
                                        (@Rules_Code
                                        ,@Rules_BeginDate
                                        ,@Prd_DescQtyBegin
                                        ,@Prd_DescQtyEnd
                                        ,@Prd_RestartQty
                                        ,@Prd_PeriodMax
                                        ,@Prd_DescUpdId
                                        ,@Prd_Desc
                                        ,@Prd_Desc_Penalty
                                        ,@Mileage
                                        ,@Prd_RemoveQty
                                        ,@Prd_RestartCycle
                                        ,@Prd_DescInfo
                                        ,@Rules_CodeOut
                                        ,@Rec_St
                                        ,@Rec_StInfo
                                        ,@Rec_InsTime
                                        ,@Rec_InsUser
                                        ,@Rec_UpdTime
                                        ,@Rec_UpdUser
		                                )
                                        SELECT @@ROWCOUNT";
                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                            messageParameters.Add("FUN", "IRC_InsDescMes");
                            messageParameters.Add("RLC", campaign.Id);
                            messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            messageParameters.Add("USC", campaign.RecordInsertionUser);
                            messageParameters.Add("PDP", discount.ThresholdPunishmentAmount);
                            messageParameters.Add("PIQ", discount.ProductInitialQuantity);
                            messageParameters.Add("PFQ", discount.ProductFinalQuantity);
                            messageParameters.Add("PPM", discount.MaximumPeriodForPunishment);
                            messageParameters.Add("PRD", discount.Discount);

                            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                            var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                            var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new MontlyProgressiveDiscountEntity()
                            {
                                Mileage = 0,
                                Prd_Desc = discount.Discount,
                                Prd_DescInfo = string.Empty,
                                Prd_DescQtyBegin = discount.ProductInitialQuantity,
                                Prd_DescQtyEnd = discount.ProductFinalQuantity,
                                Prd_DescUpdId = 'N',
                                Prd_Desc_Penalty = discount.ThresholdPunishmentAmount,
                                Prd_PeriodMax = discount.MaximumPeriodForPunishment,
                                Prd_RemoveQty = 1,
                                Prd_RestartCycle = 0,
                                Rec_InsTime = DateTime.Now,
                                Prd_RestartQty = "1",
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_St = 'A',
                                Rec_StInfo = "",
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id,
                                Rules_CodeOut = 0

                            })).ToList().First();
                        }
                    }

                }
            }
            return recordsAffected;
        }

        public async Task<int> SaveProgressiveDiscountsByThreshold(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            string sql = @"INSERT INTO [dbo].[Rules_MultiPrd]
                                   ([Rules_Code]
                                   ,[Rules_BeginDate]
                                   ,[Prd_DescQtyBegin]
                                   ,[Prd_DescQtyEnd]
                                   ,[Prd_RestartQty]
                                   ,[Prd_PeriodMax]
                                   ,[Prd_DescUpdId]
                                   ,[Prd_Desc]
                                   ,[Prd_Desc_Penalty]
                                   ,[Mileage]
                                   ,[Prd_RemoveQty]
                                   ,[Prd_RestartCycle]
                                   ,[Prd_DescInfo]
                                   ,[Rules_CodeOut]
                                   ,[Rec_St]
                                   ,[Rec_StInfo]
                                   ,[Rec_InsTime]
                                   ,[Rec_InsUser]
                                   ,[Rec_UpdTime]
                                   ,[Rec_UpdUser])
                             VALUES
                                   (@Rules_Code
                                   ,@Rules_BeginDate
                                   ,@Prd_DescQtyBegin
                                   ,@Prd_DescQtyEnd
                                   ,@Prd_RestartQty
                                   ,@Prd_PeriodMax
                                   ,@Prd_DescUpdId
                                   ,@Prd_Desc
                                   ,@Prd_Desc_Penalty
                                   ,@Mileage
                                   ,@Prd_RemoveQty
                                   ,@Prd_RestartCycle
                                   ,@Prd_DescInfo
                                   ,@Rules_CodeOut
                                   ,@Rec_St
                                   ,@Rec_StInfo
                                   ,@Rec_InsTime
                                   ,@Rec_InsUser
                                   ,@Rec_UpdTime
                                   ,@Rec_UpdUser
		                          )
                                  SELECT @@ROWCOUNT";

            var recordsAffected = 0;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync("DELETE FROM [dbo].[Rules_MultiPrd] WHERE Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate AND Prd_RestartQty = '0'", new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.Thresholds != null && campaign.Thresholds.Count() > 0)
                {


                    foreach (var threshold in campaign.Thresholds)
                    {

                        Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirPatamares");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("PIQ", threshold.InitialQuantity);
                        messageParameters.Add("PFQ", threshold.FinalQuantity);
                        messageParameters.Add("PMP", threshold.PeriodMax);
                        messageParameters.Add("PRD", threshold.Discount);
                        messageParameters.Add("PRQ", threshold.RemoveQuantity);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new ThresholdEntity()
                            {
                                Rules_Code = campaign.Id,
                                Rules_BeginDate = campaign.BeginDate,
                                Prd_DescQtyBegin = threshold.InitialQuantity,
                                Prd_DescQtyEnd = threshold.FinalQuantity,
                                Prd_RestartQty = "0",
                                Prd_PeriodMax = threshold.PeriodMax,
                                Prd_DescUpdId = 'N',
                                Prd_Desc = threshold.Discount,
                                Prd_Desc_Penalty = 0,
                                Mileage = 0,
                                Prd_RemoveQty = threshold.RemoveQuantity,
                                Prd_RestartCycle = 0,
                                Prd_DescInfo = '\0',
                                Rules_CodeOut = 0,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser

                            })).ToList().First();
                        }
                       

                        threshold.Campaign = new Campaign() { Id = campaign.Id, BeginDate = campaign.BeginDate };

                    }
                }

            }

            return recordsAffected;
        }

        public async  Task<int> SaveUnitaryProgressiveDiscounts(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            string sql = @"INSERT INTO [dbo].[Rules_MultiPrd]
                                   ([Rules_Code]
                                   ,[Rules_BeginDate]
                                   ,[Prd_DescQtyBegin]
                                   ,[Prd_DescQtyEnd]
                                   ,[Prd_RestartQty]
                                   ,[Prd_PeriodMax]
                                   ,[Prd_DescUpdId]
                                   ,[Prd_Desc]
                                   ,[Prd_Desc_Penalty]
                                   ,[Mileage]
                                   ,[Prd_RemoveQty]
                                   ,[Prd_RestartCycle]
                                   ,[Prd_DescInfo]
                                   ,[Rules_CodeOut]
                                   ,[Rec_St]
                                   ,[Rec_StInfo]
                                   ,[Rec_InsTime]
                                   ,[Rec_InsUser]
                                   ,[Rec_UpdTime]
                                   ,[Rec_UpdUser])
                             VALUES
                                   (@Rules_Code
                                   ,@Rules_BeginDate
                                   ,@Prd_DescQtyBegin
                                   ,@Prd_DescQtyEnd
                                   ,@Prd_RestartQty
                                   ,@Prd_PeriodMax
                                   ,@Prd_DescUpdId
                                   ,@Prd_Desc
                                   ,@Prd_Desc_Penalty
                                   ,@Mileage
                                   ,@Prd_RemoveQty
                                   ,@Prd_RestartCycle
                                   ,@Prd_DescInfo
                                   ,@Rules_CodeOut
                                   ,@Rec_St
                                   ,@Rec_StInfo
                                   ,@Rec_InsTime
                                   ,@Rec_InsUser
                                   ,@Rec_UpdTime
                                   ,@Rec_UpdUser
		                          )
                                  SELECT @@ROWCOUNT";

            var recordsAffected = 0;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Rules_MultiPrd] WHERE Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate " +
                        "AND Prd_RestartQty = '99'", new
                        {
                            Rules_BeginDate = campaign.BeginDate,
                            Rules_Code = campaign.Id
                        });

                if (campaign.UnitaryDiscounts != null && campaign.UnitaryDiscounts.Count() > 0)
                {
                    foreach (var discount in campaign.UnitaryDiscounts)
                    {
                        Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InsDescUnit");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("PIQ", discount.InitialQuantity);
                        messageParameters.Add("PFQ", discount.FinalQuantity);
                        messageParameters.Add("PRD", discount.Discount);
                        messageParameters.Add("PRQ", discount.PunishmentDays);
                        messageParameters.Add("PDP", discount.ThresholdPunishmentAmount);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new ProgressiveDiscountByUnityEntity()
                            {
                                Rules_Code = campaign.Id,
                                Rules_BeginDate = campaign.BeginDate,
                                Prd_DescQtyBegin = discount.InitialQuantity,
                                Prd_DescQtyEnd = discount.FinalQuantity,
                                Prd_RestartQty = "99",
                                Prd_PeriodMax = 0,
                                Prd_DescUpdId = 'N',
                                Prd_Desc = discount.Discount,
                                Prd_Desc_Penalty = discount.ThresholdPunishmentAmount,
                                Mileage = 0,
                                Prd_RemoveQty = discount.PunishmentDays,
                                Prd_RestartCycle = 0,
                                Prd_DescInfo = '\0',
                                Rules_CodeOut = 0,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser

                            })).ToList().First();
                        }
                            

                        discount.Campaign = new Campaign() { Id = campaign.Id, BeginDate = campaign.BeginDate };
                    }
                }

            }

            return recordsAffected;
        }
    }
}
