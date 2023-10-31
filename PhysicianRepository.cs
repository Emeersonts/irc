using IDP.Authorizer;
using IDP.Common;
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
    public class PhysicianRepository : IPhysicianRepository
    {
        private readonly IDataContext dataContext;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;

        public PhysicianRepository(
            IDataContext dataContext,
            ICampaignConfigurationFactory campaignConfigurationFactory)
        {
            this.dataContext = dataContext;
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async Task<int> SavePhysicians(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;
            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Rules_ProfessDetail] WHERE Rules_ProfessCode = @Rules_ProfessCode AND Acquirer_Code = @Acquirer_Code",
                       new
                       {
                           Acquirer_Code = campaign.AcquirerCode,
                           Rules_ProfessCode = campaign.Id
                       });

                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Rules_Profess] WHERE Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate",
                    new
                    {
                        Rules_BeginDate = campaign.BeginDate,
                        Rules_Code = campaign.Id
                    });
                if (campaign.Physicians != null && campaign.Products != null
                && campaign.Physicians.Count() > 0)
                {   

              
                    string sql = @" INSERT INTO [dbo].[Rules_Profess]
                                       ([Acquirer_Code]
                                       ,[Prd_Brand]
                                       ,[Rules_ProfessCode]
                                       ,[Rules_ProfessBeginDate]
                                       ,[Rules_ProfessEndDate]
                                       ,[Rules_Code]
                                       ,[Rules_BeginDate]
                                       ,[Rec_St]
                                       ,[Rec_InsTime]
                                       ,[Rec_InsUser]
                                       ,[Rec_UpdTime]
                                       ,[Rec_UpdUser])
                                 VALUES
                                       (@Acquirer_Code
                                       ,@Prd_Brand
                                       ,@Rules_ProfessCode
                                       ,@Rules_ProfessBeginDate
                                       ,@Rules_ProfessEndDate
                                       ,@Rules_Code
                                       ,@Rules_BeginDate
                                       ,@Rec_St
                                       ,@Rec_InsTime
                                       ,@Rec_InsUser
                                       ,@Rec_UpdTime
                                       ,@Rec_UpdUser
		                               )
                                       SELECT @@ROWCOUNT";

                    var brands = campaign.Products.Select(p => p.Brand.Id).Distinct();

                    foreach (var brand in brands)
                    {
                        Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirProfess");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("RED", campaign.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("PRB", brand);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {


                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new CampaignProfessionalEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                Prd_Brand = brand.ToString(),
                                Rec_St = 'A',
                                Rules_ProfessBeginDate = campaign.BeginDate,
                                Rules_ProfessEndDate = campaign.EndDate,
                                Rules_ProfessCode = campaign.Id,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id,
                            })).ToList().First();
                        }

                    }

                    foreach (var physician in campaign.Physicians)
                    {
                        sql = @"INSERT INTO [dbo].[Rules_ProfessDetail]
                                        ([Rules_ProfessCode]
                                        ,[Acquirer_Code]
                                        ,[Profess_Type]
                                        ,[Profess_Code]
                                        ,[Profess_State]
                                        ,[Rec_St]
                                        ,[Rec_InsTime]
                                        ,[Rec_InsUser]
                                        ,[Rec_UpdTime]
                                        ,[Rec_UpdUser]
                                        ,[Profess_LimitMonth]
                                        ,[Profess_CountMonth]
                                        ,[Profess_LastPrescription])
                                    VALUES
                                        (@Rules_ProfessCode
                                        ,@Acquirer_Code
                                        ,@Profess_Type
                                        ,@Profess_Code
                                        ,@Profess_State
                                        ,@Rec_St
                                        ,@Rec_InsTime
                                        ,@Rec_InsUser
                                        ,@Rec_UpdTime
                                        ,@Rec_UpdUser
                                        ,@Profess_LimitMonth
                                        ,@Profess_CountMonth
                                        ,@Profess_LastPrescription
		                                )
                                        SELECT @@ROWCOUNT";

                        Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirMedicos");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("PFC", physician.CRM);
                        messageParameters.Add("PFT", physician.ProfessionalType);
                        messageParameters.Add("STC", physician.State.Abbreviation);
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new PhysicianEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                Profess_Code = physician.CRM,
                                Profess_CountMonth = 0,
                                Profess_LimitMonth = 30,
                                Profess_LastPrescription = DateTime.Now,
                                Profess_State = physician.State.Abbreviation,
                                Profess_Type = physician.ProfessionalType,
                                Rec_St = 'A',
                                Rules_ProfessCode = campaign.Id,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
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
