using IDP.Authorizer;
using IDP.Common.Math;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using MongoDB.Driver.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class CouponRepository: ICouponRepository
    {
       
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;
        private readonly IDataContext dataContext;

        public CouponRepository(ICampaignConfigurationFactory campaignConfigurationFactory, IDataContext dataContext)
        {
            this.campaignConfigurationFactory = campaignConfigurationFactory;
            this.dataContext = dataContext;
        }

        private async Task<string> GetCampaignUsedCouponRangesByCompany(int acquirerCode)
        {
            string result = null;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                string sql = @"SELECT Acct_RangeGroup FROM AUX_RULES_COUPON where Acquirer_Code = @AcquirerCode";
                var codes = await connectionContext.QueryAsync<long>(sql, new { AcquirerCode = acquirerCode });
                if (codes.Count() > 0)
                {
                    result = string.Join(",", codes);
                }
            }
            return result;
        }

        public async Task<Coupon[]> SearchCoupon(CouponSearchParameters parameters)
            {
                var couponDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
                var coupons = new List<Coupon>();
                using (var connectionContext = couponDataContext.AcquireConnection())
                {
                    var sql = @"SELECT
                                  [Acct_RangeGroup] [RangeGroup]
                                  ,[Acct_MinNr] [MinValue]
                                  ,[Acct_MaxNr] [MaxValue]
	                              ,'Ranges ' + CONVERT(VARCHAR(20),Acct_RangeGroup) + ' De ' + CONVERT(VARCHAR(20),[Acct_MinNr]) + ' até ' + CONVERT(VARCHAR(20),[Acct_MaxNr])  as [Description]
	                              FROM [TRNCENTRE_ADV_V7].[dbo].[Str_Acct_NrControl]
	                              WHERE Acquirer_Code = @AcquirerCode AND 
	                              Rules_Code = 0  AND 
	                              ([Acct_RangeGroup] NOT IN (
									                            SELECT [Acct_RangeGroup] FROM [TRNCENTRE_ADV_V7].[dbo].[Str_Acct_NrControl] WHERE [Acct_RangeGroup] IN
									                            (SELECT vl_camp FROM [TRNCENTRE_ADV_V7].[dbo].[fn_split] (
									                            @PreviousCodes + ',0'
									                            ,',')) AND Acquirer_Code = @AcquirerCode AND Rules_Code = 0
									                            ) OR @PreviousCodes IS NULL)";
                    var previousCouponRanges = await GetCampaignUsedCouponRangesByCompany(parameters.AcquirerCode);
                    var result = await connectionContext.QueryAsync<dynamic>(sql, new { AcquirerCode = parameters.AcquirerCode, PreviousCodes = previousCouponRanges != null ? previousCouponRanges : string.Empty });

                    if(result != null && result.Count() > 0)
                    {
                            coupons = result.ToList().Select(c => new Coupon(
                           RangeKey.From(Convert.ToInt64(c.RangeGroup.ToString())),
                           Fraction.ValueOf(c.MinValue.ToString()),
                           Fraction.ValueOf(c.MaxValue.ToString()))
                            { Description = c.Description.ToString()}

                      ).ToList();
                    }
                }
             
            return coupons.ToArray();
        }

        public async Task RegisterCampaignCoupon(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[AUX_RULES_COUPON] WHERE Rules_Code = @Rules_Code AND Acquirer_Code = @Acquirer_Code",
                    new
                    {
                        Acquirer_Code = campaign.AcquirerCode,
                        Rules_Code = campaign.Id
                    });
                if (campaign.Coupon != null && campaign.Coupon.MaxValue > 0)
                 {
                    string sql = @"
                                    INSERT INTO [dbo].[AUX_RULES_COUPON]
                                           ([Rules_Code]
                                           ,[Acquirer_Code]
                                           ,[Acct_RangeGroup]
                                           ,[Acct_MinNr]
                                           ,[Acct_MaxNr])
                                     VALUES
                                           (@Rules_Code
                                           ,@Acquirer_Code
                                           ,@Acct_RangeGroup
                                           ,@Acct_MinNr
                                           ,@Acct_MaxNr)
	                                SELECT @@rowcount";
              

                        if(environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            await connectionContext.QueryAsync<int>(sql,
                              new CouponEntity()
                              {
                                  Acct_MaxNr = campaign.Coupon.MaxValue.ToDecimal(),
                                  Acct_MinNr = campaign.Coupon.MinValue.ToDecimal(),
                                  Acct_RangeGroup = campaign.Coupon.RangeKey.Value,
                                  Acquirer_Code = campaign.AcquirerCode,
                                  Rules_Code = campaign.Id
                              });
                        }
                        else
                        {
                            var messageParameters = new Dictionary<string, object>();
                            messageParameters.Add("FUN", "IRC_UpdCupom");
                            messageParameters.Add("ACQ", campaign.AcquirerCode);
                            messageParameters.Add("RLC", campaign.Id);
                            messageParameters.Add("MIN", campaign.Coupon.MinValue.ToDecimal());
                            messageParameters.Add("USC", campaign.RecordInsertionUser);
                            messageParameters.Add("CRG", campaign.Coupon.RangeKey.Value);

                            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                            var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                            var result = await authorizerRepository.ExecuteFunctionAsync(
                            authorizerMessage, Autorizer.Logix());
                        }
                       
                }
            }
        }

        public async Task<Coupon> GetDraftCouponByCampaign(CouponSearchParameters parameters )
        {
            string sql = @"SELECT 

                                      [Acct_RangeGroup]
                                       ,[Acct_MinNr]
                                       ,[Acct_MaxNr]
                           FROM [dbo].[AUX_RULES_COUPON] WHERE Rules_Code = @Id AND Acquirer_Code = @AcquirerCode";
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var result = (await connectionContext.QueryAsync<dynamic>(sql,
                    new
                    {
                        AcquirerCode = parameters.AcquirerCode,
                        Id = parameters.CampaignId
                    })).ToList().Select(coupon => new Coupon(RangeKey.From(coupon.Acct_RangeGroup),Fraction.ValueOf(Convert.ToDecimal(coupon.Acct_MinNr)), Fraction.ValueOf(Convert.ToDecimal(coupon.Acct_MaxNr))));

                if(result != null && result.Count() > 0)
                {
                    return result.First();
                }
            }

            return null;
        }
    }
}
