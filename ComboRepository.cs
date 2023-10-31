using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class ComboRepository : IComboRepository
    {
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;
        private readonly IDataContext dataContext;

        public ComboRepository (
            ICampaignConfigurationFactory campaignConfigurationFactory,
            IDataContext dataContext)
        {
            this.campaignConfigurationFactory = campaignConfigurationFactory;
            this.dataContext = dataContext;
        }

        public async Task<int> SaveBrandCombos(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"IF NOT EXISTS(SELECT Rules_Code1 FROM Rules_MixPrdBrand WHERE  Rules_Code1 = @Rules_Code1
                                      AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code
			                          AND Prd_Brand1 = @Prd_Brand1 
			                          AND Prd_Brand2 = @Prd_Brand2)
	                        BEGIN
		                        INSERT INTO [dbo].[Rules_MixPrdBrand]
                                                               ([Acquirer_Code]
                                                               ,[Rules_MixType]
                                                               ,[Rules_MixBeginDate]
                                                               ,[Rules_Code1]
                                                               ,[Prd_Brand1]
                                                               ,[Rules_Code2]
                                                               ,[Prd_Brand2]
                                                               ,[Prc_Desc1]
                                                               ,[Prc_Desc2]
                                                               ,[Rec_St]
                                                               ,[Rec_StInfo]
                                                               ,[Rec_InsTime]
                                                               ,[Rec_InsUser]
                                                               ,[Rec_UpdTime]
                                                               ,[Rec_UpdUser]
                                                               ,[Rec_SetTime]
                                                               ,[Rec_SetUser]
                                                               ,[trn_BillQty1]
                                                               ,[trn_BillQty2])
                                                         VALUES
                                                               (@Acquirer_Code
                                                               ,@Rules_MixType
                                                               ,@Rules_MixBeginDate
                                                               ,@Rules_Code1
                                                               ,@Prd_Brand1
                                                               ,@Rules_Code2
                                                               ,@Prd_Brand2
                                                               ,@Prc_Desc1
                                                               ,@Prc_Desc2
                                                               ,@Rec_St
                                                               ,@Rec_StInfo
                                                               ,@Rec_InsTime
                                                               ,@Rec_InsUser
                                                               ,@Rec_UpdTime
                                                               ,@Rec_UpdUser
                                                               ,@Rec_SetTime
                                                               ,@Rec_SetUser
                                                               ,@trn_BillQty1
                                                               ,@trn_BillQty2
		                                                       )
	                        END
	                        ELSE
	                        BEGIN 
		                        UPDATE [dbo].[Rules_MixPrdBrand]
			                            SET 
                               [Rules_Code2] = @Rules_Code2
                              ,[Prc_Desc1] = @Prc_Desc1
                              ,[Prc_Desc2] = @Prc_Desc2
                              ,[Rec_UpdTime] = GETDATE()
                              ,[Rec_UpdUser] = @Rec_UpdUser
                              ,[Rec_SetTime] = GETDATE()
                              ,[Rec_SetUser] = @Rec_UpdUser
                              ,[trn_BillQty1] = @trn_BillQty1
                              ,[trn_BillQty2] = @trn_BillQty2
			                          ,[Rec_St] = 'A'
		                         WHERE  Rules_Code1 = @Rules_Code1
                                      AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code
			                          AND Prd_Brand1 = @Prd_Brand1 
			                          AND Prd_Brand2 = @Prd_Brand2
	                        END
	                        SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sqlUpdateCombo = @"update Rules_MixPrdBrand SET Rec_St = 'D' where Rules_Code1 = @Rules_Code1
                                     AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code";
                var sqlDeleteCombo = @"DELETE FROM Rules_MixPrdBrand where Rules_Code1 = @Rules_Code1
                                     AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code";

                await connectionContext.ExecuteAsync(environmentType == CampaignEnvironmentType.Production ? sqlDeleteCombo : sqlUpdateCombo, new
                {
                    Acquirer_Code = campaign.AcquirerCode,
                    Rules_Code1 = campaign.Id,
                    Rules_MixBeginDate = campaign.BeginDate,
                });


                if (campaign.ComboBrand != null && campaign.ComboBrand.Count() > 0)
                {
                    foreach (var combo in campaign.ComboBrand)
                    {
                        var messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirComboB");
                        messageParameters.Add("RL1", campaign.Id);
                        messageParameters.Add("RL2", combo.Campaign2.Id == -1 ? campaign.Id : combo.Campaign2.Id);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("DS1", combo.Product1.Discount);
                        messageParameters.Add("DS2", combo.Product2.Discount);
                        messageParameters.Add("BR1", combo.Product1.Brand.Id);
                        messageParameters.Add("BR2", combo.Product2.Brand.Id);
                        messageParameters.Add("QT1", combo.Quantity1);
                        messageParameters.Add("QT2", combo.Quantity2);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new ComboBrandEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                Prc_Desc1 = combo.Product1.Discount,
                                Prc_Desc2 = combo.Product2.Discount,
                                Prd_Brand1 = combo.Product1.Brand.Id.ToString(),
                                Prd_Brand2 = combo.Product2.Brand.Id.ToString(),
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_SetTime = DateTime.Now,
                                Rec_SetUser = string.Empty,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,
                                Rules_Code1 = campaign.Id,
                                Rules_Code2 = combo.Campaign2.Id == -1 ? campaign.Id : combo.Campaign2.Id,
                                Rules_MixBeginDate = campaign.BeginDate,
                                Rules_MixType = 0,
                                trn_BillQty1 = combo.Quantity1,
                                trn_BillQty2 = combo.Quantity2

                            })).ToList().First();
                        }
                    }
                }
            }

            return recordsAffected;
        }

        public async Task<int> SaveCombos(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"IF NOT EXISTS(SELECT Rules_Code1 FROM Rules_MixPrdEAN WHERE  Rules_Code1 = @Rules_Code1
                                          AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code
			                              AND Prd_StdCode1 = @Prd_StdCode1 
			                              AND Prd_StdCode2 = @Prd_StdCode2)
	                            BEGIN
		                            INSERT INTO [dbo].[Rules_MixPrdEAN]
                                                               ([Acquirer_Code]
                                                               ,[Rules_MixType]
                                                               ,[Rules_MixBeginDate]
                                                               ,[Rules_Code1]
                                                               ,[Prd_StdCode1]
                                                               ,[Rules_Code2]
                                                               ,[Prd_StdCode2]
                                                               ,[Prc_Desc1]
                                                               ,[Prc_Desc2]
                                                               ,[Rec_St]
                                                               ,[Rec_StInfo]
                                                               ,[Rec_InsTime]
                                                               ,[Rec_InsUser]
                                                               ,[Rec_UpdTime]
                                                               ,[Rec_UpdUser]
                                                               ,[Rec_SetTime]
                                                               ,[Rec_SetUser]
                                                               ,[trn_BillQty1]
                                                               ,[trn_BillQty2])
                                                         VALUES
                                                               (
		                                                       @Acquirer_Code
                                                               ,@Rules_MixType
                                                               ,@Rules_MixBeginDate
                                                               ,@Rules_Code1
                                                               ,@Prd_StdCode1
                                                               ,@Rules_Code2
                                                               ,@Prd_StdCode2
                                                               ,@Prc_Desc1
                                                               ,@Prc_Desc2
                                                               ,@Rec_St
                                                               ,@Rec_StInfo
                                                               ,@Rec_InsTime
                                                               ,@Rec_InsUser
                                                               ,@Rec_UpdTime
                                                               ,@Rec_UpdUser
                                                               ,@Rec_SetTime
                                                               ,@Rec_SetUser
                                                               ,@trn_BillQty1
                                                               ,@trn_BillQty2
		                                                       )
	                            END
	                            ELSE
	                            BEGIN 
		                            UPDATE [dbo].[Rules_MixPrdEAN]
			                                SET 
                                   [Rules_Code2] = @Rules_Code2
                                  ,[Prc_Desc1] = @Prc_Desc1
                                  ,[Prc_Desc2] = @Prc_Desc2
                                  ,[Rec_UpdTime] = GETDATE()
                                  ,[Rec_UpdUser] = @Rec_UpdUser
                                  ,[Rec_SetTime] = GETDATE()
                                  ,[Rec_SetUser] = @Rec_UpdUser
                                  ,[trn_BillQty1] = @trn_BillQty1
                                  ,[trn_BillQty2] = @trn_BillQty2
			                              ,[Rec_St] = 'A'
		                             WHERE  Rules_Code1 = @Rules_Code1
                                          AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code
			                              AND Prd_StdCode1 = @Prd_StdCode1 
			                              AND Prd_StdCode2 = @Prd_StdCode2
	                            END
	                            SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sqlDeleteCombo = @"UPDATE Rules_MixPrdEAN SET Rec_St = 'D' where Rules_Code1 = @Rules_Code1
                                     AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code";
                var sqlUpdateCombo = @"DELETE FROM Rules_MixPrdEAN where Rules_Code1 = @Rules_Code1
                                     AND Rules_MixBeginDate = @Rules_MixBeginDate AND Acquirer_Code = @Acquirer_Code";
                await connectionContext.ExecuteAsync(environmentType.Equals(CampaignEnvironmentType.Production)? 
                    sqlDeleteCombo:sqlUpdateCombo, new
                {
                    Acquirer_Code = campaign.AcquirerCode,
                    Rules_Code1 = campaign.Id,
                    Rules_MixBeginDate = campaign.BeginDate
                });

                if (campaign.Combos != null && campaign.Combos.Length > 0)
                {
                    foreach (var combo in campaign.Combos)
                    {
                        var messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirCombos");
                        messageParameters.Add("RL1", campaign.Id);
                        messageParameters.Add("RL2", combo.Campaign2.Id == -1 ? campaign.Id : combo.Campaign2.Id);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("DS1", combo.Product1.Discount);
                        messageParameters.Add("DS2", combo.Product2.Discount);
                        messageParameters.Add("EA1", combo.Product1.EAN);
                        messageParameters.Add("EA2", combo.Product2.EAN);
                        messageParameters.Add("QT1", combo.Quantity1);
                        messageParameters.Add("QT2", combo.Quantity2);


                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new ComboCampaignEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                Prc_Desc1 = combo.Product1.Discount,
                                Prc_Desc2 = combo.Product2.Discount,
                                Prd_StdCode1 = Convert.ToDecimal(combo.Product1.EAN),
                                Prd_StdCode2 = Convert.ToDecimal(combo.Product2.EAN),
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_SetTime = DateTime.Now,
                                Rec_SetUser = string.Empty,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,
                                Rules_Code1 = campaign.Id,
                                Rules_Code2 = combo.Campaign2.Id == -1 ? campaign.Id : combo.Campaign2.Id,
                                Rules_MixBeginDate = campaign.BeginDate,
                                Rules_MixType = 0,
                                trn_BillQty1 = combo.Quantity1,
                                trn_BillQty2 = combo.Quantity2

                            })).ToList().First();
                        }
                    }
                }

            }

            return recordsAffected;
        }
    }
}
