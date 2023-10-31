using Dapper;
using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;
        private readonly ICityRepository cityRepository;
        private readonly IDataContext dataContext;
        private readonly ICompanyProgramRepository companyProgramRepository;
        private readonly IMembershipChannelRepository membershipChannelRepository;
        private readonly IProductRepository productRepository;
        private readonly IProgressiveDiscountRepository progressiveDiscountRepository;
        private readonly IPhysicianRepository physicianRepository;
        private readonly IQuestionnaireRepository questionnaireRepository;
        private readonly IComboRepository comboRepository;
        private readonly IKitRepository kitRepository;
        private readonly INetworkRepository networkRepository;
        private readonly ICouponRepository couponRepository;

        public CampaignRepository(
            ICampaignConfigurationFactory campaignConfigurationFactory,
            ICityRepository cityRepository,
            IDataContext dataContext,
            ICompanyProgramRepository companyProgramRepository,
            IProductRepository productRepository,
            IProgressiveDiscountRepository progressiveDiscountRepository,
            IKitRepository kitRepository,
            IComboRepository comboRepository,
            IMembershipChannelRepository membershipChannelRepository,
            INetworkRepository networkRepository,
            IPhysicianRepository physicianRepository,
            IQuestionnaireRepository questionnaireRepository,
            ICouponRepository couponRepository)
        {
            this.campaignConfigurationFactory = campaignConfigurationFactory;
            this.cityRepository = cityRepository;
            this.dataContext = dataContext;
            this.companyProgramRepository = companyProgramRepository;
            this.membershipChannelRepository = membershipChannelRepository;
            this.productRepository = productRepository;
            this.progressiveDiscountRepository = progressiveDiscountRepository;
            this.comboRepository = comboRepository;
            this.kitRepository = kitRepository;
            this.networkRepository = networkRepository;
            this.physicianRepository = physicianRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.couponRepository = couponRepository;
        }

        private async Task<CampaignView[]> SearchCampaigns(CampaignSearchParameters parameters)
        {
            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            var campaigns = new List<CampaignView>();
            var sql = @"SELECT rf.Acct_Nr [ProgramId],
                                    rf.Rules_Code [Id],
                                    rf.Rules_Name [Name],
                                    rf.Rules_BeginDate [BeginDate],
                                    rf.Rules_EndDate [EndDate],
                                     case
					                  when rf.Rec_St = 'D' THEN 'DESATIVADA'
                                      when  rf.rules_enddate < getdate() or rf.rec_st IN  ('P')  THEN 'EXPIRADA'
                                      when   (rf.rules_begindate <= getdate() OR rf.Rules_BeginDate > GETDATE()) and rf.rules_enddate > getdate()  and rf.rec_st = 'A' then 'ATIVO'
                                    end as [Status],
                                    RTRIM(sa.Acct_Name) [ProgramName] 
			                   from [TRNCENTRE_ADV_V7].[dbo].rules_msfile rf 
		                 INNER JOIN [TRNCENTRE_ADV_V7].[dbo].str_acct_parms sa
                                 ON rf.Acct_Nr = sa.Acct_nr
                             where rf.acquirer_code = @AcquirerCode
			                 AND sa.acquirer_code = @AcquirerCode
			                 AND rf.Rec_St IN ('A','D')";

            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var result = await connectionContext.QueryAsync<CampaignView>(sql, new { AcquirerCode = parameters.AcquirerCode });
                if (result != null && result.Count() > 0)
                {
                    return result.ToArray();
                }

            }


            return campaigns.ToArray();

        }


        public async Task<CampaignViewStatus[]> GetActiveCampaigns(string acquirerCode)
        {
            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            var campaigns = new List<CampaignViewStatus>();
            var sql = @"SELECT          
                        rf.Rules_Code [Id],
                            RTRIM(rf.TktMsg_RulesInfo) + ' - ' + rf.Rules_Name [Name],
                            rf.Rules_BeginDate [BeginDate],
                            rf.Rules_EndDate [EndDate]
	                    FROM [TRNCENTRE_ADV_V7].[dbo].rules_msfile rf
                        WHERE rf.Rec_St = 'A'
	                      AND rf.Rules_EndDate > GETDATE()
	                      AND rf.Rules_LastDate > GETDATE()
	                      AND rf.Acquirer_Code = @AcquirerCode";
            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var result = await connectionContext.QueryAsync<CampaignViewStatus>(sql, new { AcquirerCode = acquirerCode });
                if (result != null && result.Any())
                {
                    return result.ToArray();
                }
            }


            return campaigns.ToArray();
        }


        private async Task<int> GetNextCampaignCode(int acquirerCode, string previousCodes)
        {
            var campaignId = 1;
            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var sql = @"SELECT * FROM  (
	                        SELECT Rules_CodeOut As Id FROM TRNCENTRE_ADV_V7.dbo.Rules_Migration UNION
		                    SELECT Rules_CodeIn  FROM TRNCENTRE_ADV_V7.dbo.Rules_Migration UNION
		                    SELECT Rules_Code FROM TRNCENTRE_ADV_V7.dbo.Rules_MsFile WHERE Acquirer_Code = @AcquirerCode) AS A ";
                var auxIds = await connectionContext.QueryAsync<int>(sql, new { AcquirerCode = acquirerCode });
                if (auxIds != null && auxIds.Count() > 0)
                {
                    var ids = auxIds.ToList();
                    if (!string.IsNullOrWhiteSpace(previousCodes))
                    {
                        ids.AddRange(previousCodes.Split(',').Select(int.Parse).ToList());
                    }
                    var maxId = ids.Max() + 1;
                    var auxList = Enumerable.Range(1, maxId);

                    auxList = auxList.Except(ids);
                    campaignId = auxList.ToList().Min();
                }
            }
            return campaignId;
        }

        public async Task<CampaignView[]> GetCampaignsByAcquirerCode(CampaignSearchParameters parameters)
        {
            var activeCampaigns = await SearchCampaigns(parameters);

            var campaignList = new List<CampaignView>();

            var sql =
                @"select RequestId from ApprovalRequest WHERE Status = 'A' AND Acquirer_Code = @acquirerCode";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var expiredCampaings = activeCampaigns.ToList().FindAll(expiredCampaign =>
                expiredCampaign.Status.ToUpper().Equals(Enum.GetName(typeof(CampaignStatus), CampaignStatus.EXPIRADA)));

                var deactivatedCampaigns = activeCampaigns.ToList().FindAll(deactivatedCampaign =>
                deactivatedCampaign.Status.ToUpper().Equals(Enum.GetName(typeof(CampaignStatus), CampaignStatus.DESATIVADA))).ToArray();

                activeCampaigns = activeCampaigns.ToList().FindAll(expiredCampaign =>
                expiredCampaign.Status.ToUpper() != Enum.GetName(typeof(CampaignStatus), CampaignStatus.EXPIRADA) &&
                expiredCampaign.Status != Enum.GetName(typeof(CampaignStatus), CampaignStatus.DESATIVADA)).ToArray();

                sql = @"select 
                        DISTINCT rf.Acct_Nr [ProgramId],
                        rf.Rules_Code [Id],
	                    rf.Rules_Name [Name],
	                    rf.Rules_BeginDate [BeginDate],
	                    rf.Rules_EndDate [EndDate],
                        'Rascunho' AS [Status]
                        from [dbo].rules_msfile rf

                    where rf.acquirer_code = @acquirerCode AND 
                        rf.Rec_St = 'A' AND 
					    rf.Rules_Code NOT IN (SELECT RequestId from ApprovalRequest WHERE rf.Acquirer_Code = @acquirerCode)
                    order by rf.Rules_BeginDate DESC";

                var draftCampaigns = await connectionContext.QueryAsync<CampaignView>(
                        sql, new { acquirerCode = parameters.AcquirerCode });

                sql = @"select 
                        DISTINCT rf.Acct_Nr [ProgramId],
                        rf.Rules_Code [Id],
	                    rf.Rules_Name [Name],
	                    rf.Rules_BeginDate [BeginDate],
	                    rf.Rules_EndDate [EndDate],
                        'Em Aprovação' AS [Status]
                        from [dbo].rules_msfile rf

                            where rf.acquirer_code = @acquirerCode AND 
                        rf.Rec_St = 'A' AND 
					    rf.Rules_Code IN (
                                        SELECT 
                                                Approval.RequestId 
                                        FROM ApprovalRequest Approval 
                                        WHERE [Status] = 'P' AND 
                                        Approval.Acquirer_Code = @acquirerCode  
                                        AND RecInsTime = (SELECT MAX(RecInsTime) FROM
                                        ApprovalRequest WHERE RequestId = Approval.RequestId AND	
										Acquirer_Code = @acquirerCode AND
                                        [Status] = 'P' )
                                        )
                    order by rf.Rules_BeginDate DESC";

                var onApproval = await connectionContext.QueryAsync<CampaignView>(
                        sql, new { acquirerCode = parameters.AcquirerCode });



                sql = @"select 
                        DISTINCT rf.Acct_Nr [ProgramId],
                        rf.Rules_Code [Id],
	                    rf.Rules_Name [Name],
	                    rf.Rules_BeginDate [BeginDate],
	                    rf.Rules_EndDate [EndDate],
                        'Ativo' AS [Status]
                        from [dbo].rules_msfile rf

                            where rf.acquirer_code = @acquirerCode AND 
                        rf.Rec_St = 'A' AND 
					    rf.Rules_Code IN (
                                        SELECT 
                                                Approval.RequestId 
                                        FROM ApprovalRequest Approval 
                                        WHERE [Status] = 'A' AND 
                                        Approval.Acquirer_Code = @acquirerCode  
                                        AND RecInsTime = (SELECT MAX(RecInsTime) FROM
                                        ApprovalRequest WHERE RequestId = Approval.RequestId
                                        AND	
										Acquirer_Code = @acquirerCode AND
                                        [Status] = 'A')
                                        )
                    order by rf.Rules_BeginDate DESC";

                sql = @"select 
                            DISTINCT rf.Acct_Nr [ProgramId],
                            rf.Rules_Code [Id],
	                        rf.Rules_Name [Name],
	                        rf.Rules_BeginDate [BeginDate],
	                        rf.Rules_EndDate [EndDate],
                            'Recusado' AS [Status],
							ApprovalRequest.RequestInfo [ReasonForRefusal]
                            from [dbo].rules_msfile rf INNER JOIN
							ApprovalRequest ON
							rf.Rules_Code = ApprovalRequest.RequestId
							AND ApprovalRequest.[Status] = 'R'

                             where rf.acquirer_code = @acquirerCode AND 
                            rf.Rec_St = 'A' AND 
					        rf.Rules_Code IN (
                                            SELECT 
                                                 Approval.RequestId 
                                            FROM ApprovalRequest Approval 
                                            WHERE [Status] = 'R' AND 
                                            Approval.Acquirer_Code = @acquirerCode  
                                            AND RecInsTime = (SELECT MAX(RecInsTime) FROM
                                            ApprovalRequest WHERE RequestId = Approval.RequestId AND	
											Acquirer_Code = @acquirerCode AND
											[Status] = 'R'
											)
                                           )
                        order by rf.Rules_BeginDate DESC";

                var refused = await connectionContext.QueryAsync<CampaignView>(
                     sql, new { acquirerCode = parameters.AcquirerCode });

                activeCampaigns = activeCampaigns.OrderByDescending(c => c.BeginDate).ToArray();
                campaignList.AddRange(activeCampaigns);
                campaignList = campaignList.OrderByDescending(c => c.BeginDate).ToList();
                campaignList.AddRange(onApproval);
                campaignList.AddRange(draftCampaigns);
                campaignList.AddRange(refused);
                campaignList.AddRange(expiredCampaings);
                campaignList.AddRange(deactivatedCampaigns);

                var campaigns = campaignList.ToArray();

                var programIds = campaigns.Select(c => c.ProgramId).Distinct();

                Dictionary<int, string> programs = (await companyProgramRepository.GetProgramsById(new CompanyProgramSearchParameters()
                {
                    AcquirerCode = Convert.ToInt32(parameters.AcquirerCode),
                    ProgramIds = string.Join(",", programIds)
                })).ToDictionary(x => x.Id, x => x.Name);

                if (programs != null && programs.Count > 0)
                {
                    foreach (var c in campaigns)
                    {
                        c.ProgramName = programs[c.ProgramId];
                    }
                }



                campaignList = campaigns.ToList();
            }


            return campaignList.ToArray();

        }

        private async Task<string> GetAdministratorNameBy(int acquirerCode)
        {
            var acquirerName = string.Empty;
            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var sql = @"SELECT Acquirer_NickName [Name]
		                    FROM TRNCENTRE_ADV_V7.dbo.Str_Acquirer
		                    WHERE Acquirer_Code = @AcquirerCode";
                var result = await connectionContext.QueryAsync<string>(sql, new { AcquirerCode = acquirerCode });
                if (result != null && result.Any())
                {
                    return result.First();
                }
            }


            return acquirerName;
        }


        private async Task<int> RegisterAssociatedCampaigns(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"INSERT INTO [dbo].[Rules_Append]
                                   ([Acquirer_Code]
                                   ,[Rules_Code]
                                   ,[Rules_BeginDate]
                                   ,[RulesAppend_BeginTime]
                                   ,[RulesAppend_EndTime]
                                   ,[Rules_CodeAppend]
                                   ,[Rules_Append_BeginDate]
                                   ,[Rec_InsTime]
                                   ,[Rec_InsUser]
                                   ,[Rec_UpdTime]
                                   ,[Rec_UpdUser])
                             VALUES
                                   (@Acquirer_Code
                                   ,@Rules_Code
                                   ,@Rules_BeginDate
                                   ,@RulesAppend_BeginTime
                                   ,@RulesAppend_EndTime
                                   ,@Rules_CodeAppend
                                   ,@Rules_Append_BeginDate
                                   ,@Rec_InsTime
                                   ,@Rec_InsUser
                                   ,@Rec_UpdTime
                                   ,@Rec_UpdUser
		                           )
                                    SELECT @@ROWCOUNT";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Rules_Append] WHERE Acquirer_Code = @Acquirer_Code
                    AND Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate", new
                {
                    Acquirer_Code = campaign.AcquirerCode,
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.AssociatedCampaigns != null && campaign.AssociatedCampaigns.Count() > 0)
                {
                    foreach (var associatedCampaign in campaign.AssociatedCampaigns)
                    {
                        var messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirAssoci");
                        messageParameters.Add("ACQ", campaign.AcquirerCode);
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("RAB", associatedCampaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("RAE", associatedCampaign.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageParameters.Add("RCA", associatedCampaign.Id);
                        messageParameters.Add("USC", campaign.RecordInsertionUser);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        var insertResult = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new AssociatedCampaignEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                RulesAppend_BeginTime = associatedCampaign.BeginDate,
                                RulesAppend_EndTime = associatedCampaign.EndDate,
                                Rules_Append_BeginDate = associatedCampaign.BeginDate,
                                Rules_CodeAppend = associatedCampaign.Id,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser,
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id,
                            })).ToList().First();
                        }

                    }
                }


                return recordsAffected;
            }
        }

        private async Task<int> RegisterCampaignMigrations(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;

            string sql = @"INSERT INTO [dbo].[Rules_Migration]
                                   ([Migration_Id]
                                   ,[Rules_CodeIn]
                                   ,[Rules_CodeOut]
                                   ,[Migration_Type]
                                   ,[Migration_QtyDscDown]
                                   ,[Migration_QtyDscUp]
                                   ,[Migration_QtyDscEqual]
                                   ,[Migration_QtyNewInsert]
                                   ,[Migration_QtyNewIgnore]
                                   ,[Migration_QtyOldOk]
                                   ,[Migration_QtyOldDelete]
                                   ,[Migration_LastHolder]
                                   ,[Rec_St]
                                   ,[Rec_InsTime]
                                   ,[Rec_InsUser]
                                   ,[Rec_UpdTime]
                                   ,[Rec_UpdUser])
                             VALUES
                                   (@Migration_Id
                                   ,@Rules_CodeIn 
                                   ,@Rules_CodeOut 
                                   ,@Migration_Type
                                   ,@Migration_QtyDscDown
                                   ,@Migration_QtyDscUp
                                   ,@Migration_QtyDscEqual 
                                   ,@Migration_QtyNewInsert 
                                   ,@Migration_QtyNewIgnore 
                                   ,@Migration_QtyOldOk 
                                   ,@Migration_QtyOldDelete 
                                   ,@Migration_LastHolder 
                                   ,@Rec_St
                                   ,@Rec_InsTime
                                   ,@Rec_InsUser
                                   ,@Rec_UpdTime
                                   ,@Rec_UpdUser
		                           )
                                  SELECT @@ROWCOUNT";

            var sqlMigrationMapping = @"INSERT INTO [dbo].[MigrationMapping]
                                               ([Rules_Code]
                                               ,[Acquirer_Code]
                                               ,[Rules_BeginDate]
                                               ,[Migration_Id])
                                         VALUES
                                               (@Rules_Code
                                               ,@Acquirer_Code
                                               ,@Rules_BeginDate
                                               ,@Migration_Id
		                                       )";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Rules_Migration] WHERE Migration_Id IN (
		            SELECT Migration_Id FROM MigrationMapping WHERE Acquirer_Code = @AcquirerCode AND
		            Rules_Code = @Id AND Rules_BeginDate = @BeginDate
	            )
	            DELETE FROM MigrationMapping WHERE Acquirer_Code = @AcquirerCode AND
                Rules_Code = @Id AND Rules_BeginDate = @BeginDate", new
                {
                    AcquirerCode = campaign.AcquirerCode,
                    Id = campaign.Id,
                    BeginDate = campaign.BeginDate
                });

                if (campaign.MigratedCampaigns != null && campaign.MigratedCampaigns.Count() > 0)
                {
                    foreach (var migration in campaign.MigratedCampaigns)
                    {
                        var migratioId = await GetMigradionKey();
                        var messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InserirMigracoes");
                        messageParameters.Add("RLI", migration.CodeIn);
                        messageParameters.Add("RLO", campaign.Id);
                        messageParameters.Add("MRC", migratioId);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);
                        messageParameters.Add("RBD", campaign.BeginDate);
                        messageParameters.Add("USC", campaign.RecordInsertionUser);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                        var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                        await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new CampaignMigrationEntity()
                            {
                                Migration_Id = migratioId,
                                Rules_CodeIn = migration.CodeIn,
                                Rules_CodeOut = campaign.Id,
                                Migration_Type = "CMP",
                                Migration_QtyDscDown = 0,
                                Migration_QtyDscUp = 0,
                                Migration_QtyDscEqual = 0,
                                Migration_QtyNewInsert = 0,
                                Migration_QtyNewIgnore = 0,
                                Migration_QtyOldOk = 0,
                                Migration_QtyOldDelete = 0,
                                Migration_LastHolder = 0,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_St = 'A',
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser
                            })).ToList().First();

                            await connectionContext.ExecuteAsync(sqlMigrationMapping, new MigrationMappingEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                Migration_Id = migratioId,
                                Rules_BeginDate = campaign.BeginDate,
                                Rules_Code = campaign.Id
                            });
                        }
                    }
                }

            }

            return recordsAffected;
        }

        private async Task<int> GetMigradionKey()
        {
            var migrationcount = 0;

            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            var query = @"SELECT MAX(Migration_Id) FROM [TRNCENTRE_ADV_V7].dbo.Rules_Migration";

            using (var connectionContext = campaignDataContext.AcquireConnection())
            {
                var migration = (await connectionContext.QueryAsync<int>(query));
                migrationcount = Convert.ToInt32(migration.FirstOrDefault()) + 1;
            }

            return migrationcount;
        }

        public async Task<int> RegisterCampaign(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var campaignCode = 0;
            var acquirerName = await GetAdministratorNameBy(campaign.AcquirerCode);

            if(campaign.Id > 0)
            {
                await ClearCampaign(campaign);
            }

            if (environmentType.Equals(CampaignEnvironmentType.Production))
            {
                campaignCode = campaign.Id;
                await RegisterOnLogix(campaign, acquirerName, environmentType, campaignCode);
                using(var connectionContextDesenv = dataContext.AcquireConnection())
                {
                    await connectionContextDesenv.ExecuteAsync(@"DELETE FROM  [dbo].[Rules_AdherenceModel] WHERE 
                    [Rules_Code] = @Rules_Code AND Acquirer_Code = @Acquirer_Code", new
                    {
                        Acquirer_Code = campaign.AcquirerCode,
                        Rules_Code = campaign.Id
                    });

                    await connectionContextDesenv.ExecuteAsync(@"DELETE FROM [dbo].[Rules_MsFile] WHERE 
                    [Rules_Code] = @Rules_Code AND Acquirer_Code = @Acquirer_Code", new
                    {
                        Acquirer_Code = campaign.AcquirerCode,
                        Rules_Code = campaign.Id
                    });
                }
            }
            else
            {
                var previousCodes = await GetCampaignUsedCodesByCompany(campaign.AcquirerCode);

                campaignCode = campaign.Id == 0 ? await GetNextCampaignCode(campaign.AcquirerCode, previousCodes) : campaign.Id;
                await RegisterOnLogix(campaign, acquirerName, environmentType, campaignCode);

                using (var connectionContext = dataContext.AcquireConnection())
                {
                    if (campaign.Id > 0)
                    {

                        await connectionContext.ExecuteAsync(@"DELETE FROM  [dbo].[Rules_AdherenceModel] WHERE 
                    [Rules_Code] = @Rules_Code AND Acquirer_Code = @Acquirer_Code", new
                        {
                            Acquirer_Code = campaign.AcquirerCode,
                            Rules_Code = campaign.Id
                        });

                    }

                    string sql =
                           @"IF NOT EXISTS(SELECT Rules_Code FROM Rules_MsFile WHERE Acquirer_Code = @Acquirer_Code AND
	                            Rules_Code = @Rules_Code AND Rules_BeginDate = @Rules_BeginDate)
	                            BEGIN
		                            INSERT INTO [dbo].[Rules_MsFile]
                                                   ([Acquirer_Code]
                                                   ,[Rules_Code]
                                                   ,[Rules_BeginDate]
                                                   ,[Rules_EndDate]
                                                   ,[Rules_LastDate]
                                                   ,[Rules_VersionId]
                                                   ,[Rules_Name]
                                                   ,[Rules_QtyMaxMember]
                                                   ,[Rules_QtyCurrentMember]
                                                   ,[Rules_UpdId]
                                                   ,[Rules_BillKitNrDays]
                                                   ,[Rules_AgrKitNrDays]
                                                   ,[Rules_ExpKitNrDays]
                                                   ,[Acct_Nr]
                                                   ,[TktMsg_Id]
                                                   ,[TktMsg_RulesInfo]
                                                   ,[Rules_PrdAdhesionId]
                                                   ,[Holder_CodeMode]
                                                   ,[Rules_LimitMode]
                                                   ,[Rules_BillNrDays]
                                                   ,[Rules_Type]
                                                   ,[Rules_ControlMode]
                                                   ,[Rules_ExclusiveId]
                                                   ,[Rec_St]
                                                   ,[Rec_StInfo]
                                                   ,[Rec_InsTime]
                                                   ,[Rec_InsUser]
                                                   ,[Rec_UpdTime]
                                                   ,[Rec_UpdUser]
                                                   ,[Prd_QtyLevelId]
                                                   ,[Rules_PrdAdhesionActiveId]
                                                   ,[Rules_DescStateId]
                                                   ,[Rule_PriceDesc]
                                                   ,[QtyPriceDescMax])
                                             VALUES
                                                   (@Acquirer_Code
                                                   ,@Rules_Code
                                                   ,@Rules_BeginDate
                                                   ,@Rules_EndDate
                                                   ,@Rules_LastDate
                                                   ,@Rules_VersionId
                                                   ,@Rules_Name
                                                   ,@Rules_QtyMaxMember
                                                   ,@Rules_QtyCurrentMember
                                                   ,@Rules_UpdId
                                                   ,@Rules_BillKitNrDays
                                                   ,@Rules_AgrKitNrDays
                                                   ,@Rules_ExpKitNrDays
                                                   ,@Acct_Nr
                                                   ,@TktMsg_Id
                                                   ,@TktMsg_RulesInfo
                                                   ,@Rules_PrdAdhesionId
                                                   ,@Holder_CodeMode
                                                   ,@Rules_LimitMode
                                                   ,@Rules_BillNrDays
                                                   ,@Rules_Type
                                                   ,@Rules_ControlMode
                                                   ,@Rules_ExclusiveId
                                                   ,@Rec_St
                                                   ,@Rec_StInfo
                                                   ,@Rec_InsTime
                                                   ,@Rec_InsUser
                                                   ,@Rec_UpdTime
                                                   ,@Rec_UpdUser
                                                   ,@Prd_QtyLevelId
                                                   ,@Rules_PrdAdhesionActiveId
                                                   ,@Rules_DescStateId
                                                   ,@Rule_PriceDesc
                                                   ,@QtyPriceDescMax)
	                            END
	                            ELSE
	                            BEGIN 
		                            UPDATE [dbo].[Rules_MsFile]
		                               SET
			                              [Rules_EndDate] = @Rules_EndDate
			                              ,[Rules_LastDate] = @Rules_LastDate
			                              ,[Rules_Name] = @Rules_Name
			                              ,[Rules_QtyMaxMember] = @Rules_QtyMaxMember
			                              ,[Acct_Nr] = @Acct_Nr
			                              ,[Rules_LimitMode] = @Rules_LimitMode
			                              ,[Rules_BillNrDays] = @Rules_BillNrDays
			                              ,[Rules_ControlMode] = @Rules_ControlMode
			                              ,[Rec_UpdTime] = GETDATE()
			                              ,[Rec_UpdUser] = @Rec_UpdUser
			                              ,[Prd_QtyLevelId] = @Prd_QtyLevelId
			                              ,[Rules_DescStateId] = @Rules_DescStateId
			                              ,[Rule_PriceDesc] = @Rule_PriceDesc
			                              ,[QtyPriceDescMax] = @QtyPriceDescMax
		                             WHERE Rules_Code = @Rules_Code AND Acquirer_Code = @Acquirer_Code AND Rules_BeginDate  = @Rules_BeginDate
	                            END
	                            SELECT  @Rules_Code";

                    await connectionContext.QueryAsync<int>(sql, new CampaignEntity()
                    {
                        Acquirer_Code = campaign.AcquirerCode,
                        Rules_Code = campaignCode,
                        Rules_BeginDate = campaign.BeginDate,
                        Rules_LastDate = campaign.EndDateForPurchase,
                        Rules_QtyMaxMember = campaign.MaximumNumberOfParticipants,
                        Rules_EndDate = campaign.EndDate,
                        Rules_Name = campaign.Name,
                        Rules_VersionId = 'N',
                        Rules_DescStateId = campaign.StateDiscountType,
                        Rules_QtyCurrentMember = 0,
                        Acct_Nr = campaign.Program.Id,
                        Holder_CodeMode = '1',
                        Prd_QtyLevelId = campaign.ThresholdControl != null ? campaign.ThresholdControl[0] : 'N',
                        QtyPriceDescMax = campaign.QuantityPriceDescMax,
                        Rec_InsTime = DateTime.Now,
                        Rec_InsUser = campaign.RecordInsertionUser,
                        Rec_St = 'A',
                        Rec_StInfo = string.Empty,
                        Rec_UpdTime = DateTime.Now,
                        Rec_UpdUser = campaign.RecordInsertionUser,
                        Rules_AgrKitNrDays = 0,
                        Rules_BillKitNrDays = 0,
                        Rules_BillNrDays = campaign.BillKitNumberOfDays,
                        Rules_ControlMode = campaign.ControlMode,
                        Rules_ExclusiveId = 1,
                        Rules_ExpKitNrDays = 0,
                        Rules_LimitMode = campaign.LimitMode,
                        Rules_PrdAdhesionActiveId = 'Y',
                        Rules_PrdAdhesionId = campaign.ProductdAdhesionId,
                        Rules_Type = 0,
                        Rules_UpdId = 'N',
                        Rule_PriceDesc = campaign.PriceDesc,
                        TktMsg_Id = 'N',
                        TktMsg_RulesInfo = acquirerName
                    });


                    var adherenceModelInsertMap = new Dictionary<string, string>() {
                        {"S",@"Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA2', '4','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA4', '2','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          SELECT  @@RowCount" },
                        {"SCRM",@"Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA1', '1','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                           Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA3', '3','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                           Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA5', '1','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                            SELECT @@ROWCOUNT" },
                        {"C",@"Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA4', '1','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          SELECT @@ROWCOUNT" },
                        {"CCRM",@"Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA1', '5','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                                            Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA5', '1','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          SELECT @@ROWCOUNT" },
                        {"SSG",@"Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA6', '1','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          SELECT @@ROWCOUNT" },
                        {"CSE",@"Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA1', '5','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          Insert into rules_adherencemodel values (@Acquirer_code,@Rules_code,'ATIVA7', '1','A','CM999',GETDATE(),'CM999',GETDATE(),'N')
                          SELECT @@ROWCOUNT" }
                    };

                    var insertAdherecenModelSql = adherenceModelInsertMap[campaign.AdhrenceModel.AdherenceDataType];


                    await connectionContext.QueryAsync<int>(insertAdherecenModelSql, new { Acquirer_code = campaign.AcquirerCode, Rules_code = campaignCode });

                    campaign.Id = campaignCode;
                }
            }

           

            if (campaignCode > 0)
            {
                campaign.Id = campaignCode;
                await this.progressiveDiscountRepository.SaveProgressiveDiscountsByThreshold(campaign, environmentType);
                await this.productRepository.SaveProducts(campaign, environmentType);
                await this.comboRepository.SaveCombos(campaign, environmentType);
                await this.comboRepository.SaveBrandCombos(campaign, environmentType);
                await this.kitRepository.SaveKits(campaign, environmentType);
                await this.RegisterCampaignMigrations(campaign, environmentType);
                await this.membershipChannelRepository.SaveChannels(campaign, environmentType);
                await this.networkRepository.SaveNetworks(campaign, environmentType);
                await this.progressiveDiscountRepository.SaveMonthlyProgressiveDiscounts(campaign, environmentType);
                await this.progressiveDiscountRepository.SaveUnitaryProgressiveDiscounts(campaign, environmentType);
                await this.RegisterAssociatedCampaigns(campaign, environmentType);
                await this.networkRepository.SaveNetworkDiscounts(campaign, environmentType);
                await this.physicianRepository.SavePhysicians(campaign, environmentType);
                await this.questionnaireRepository.SaveQuestionnaire(campaign, environmentType);
                await this.couponRepository.RegisterCampaignCoupon(campaign, environmentType);
            }

            return campaignCode;
        }

        private async Task<bool> RegisterOnLogix(Campaign campaign, string acquirerName, CampaignEnvironmentType environmentType, int campaignCode)
        {
            var messageParameters = new Dictionary<string, object>();

            messageParameters.Add("FUN", "IRC_InserirCampanha");
            messageParameters.Add("ACQ", campaign.AcquirerCode);
            messageParameters.Add("RLC", campaign.Id > 0 ?campaign.Id:campaignCode);
            messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("RED", campaign.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("RLD", campaign.EndDateForPurchase.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("PRC", campaign.Program.Id);
            messageParameters.Add("QLC", campaign.ThresholdControl != null ? campaign.ThresholdControl[0] : 'N');
            messageParameters.Add("PDM", campaign.QuantityPriceDescMax);
            messageParameters.Add("USC", campaign.RecordInsertionUser);
            messageParameters.Add("BND", campaign.BillKitNumberOfDays);
            messageParameters.Add("RCM", campaign.ControlMode);
            messageParameters.Add("CLM", campaign.LimitMode.ToString());
            messageParameters.Add("PAC", campaign.ProductdAdhesionId.Equals('\0') ? " " : campaign.ProductdAdhesionId.ToString());
            messageParameters.Add("RPD", campaign.PriceDesc.Equals('\0') ? " " : campaign.PriceDesc.ToString());
            messageParameters.Add("RLN", campaign.Name.ToString());
            messageParameters.Add("QMM", campaign.MaximumNumberOfParticipants.ToString());
            messageParameters.Add("ACN", acquirerName);
            messageParameters.Add("RDS", campaign.StateDiscountType);
            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
            var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
            await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());


            if(campaign.AdhrenceModel != null)
            {
                var parameters = new Dictionary<string, object>();
                parameters.Add("FUN", "IRC_InserirAdesao");
                parameters.Add("ACQ", campaign.AcquirerCode);
                parameters.Add("RLC", campaign.Id > 0 ? campaign.Id : campaignCode);
                parameters.Add("ADT", campaign.AdhrenceModel.AdherenceDataType);

                authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(parameters));
                await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
            }

            return  true;
        }

        private async Task ClearCampaign(Campaign campaign)
        {
            var messageParameters = new Dictionary<string, object>();

            messageParameters.Add("FUN", "IRC_LimparCampanha");
            messageParameters.Add("ACQ", campaign.AcquirerCode);
            messageParameters.Add("RLC", campaign.Id);
            messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));

            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
            var authorizerRepository = campaignConfigurationFactory.Create(CampaignEnvironmentType.Homolog).CreateConnection();

            await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
        }


        private async Task<string> GetCampaignUsedCodesByCompany(int acquirerCode)
        {
            string result = null;
            using (var connectionContext = dataContext.AcquireConnection())
            {
                string sql = @"SELECT Rules_CodeIn AS Rules_Code FROM Rules_Migration UNION
                               SELECT Rules_CodeOut AS Rules_Code FROM Rules_Migration UNION
                               SELECT Rules_Code 
                                FROM Rules_MsFile
                                WHERE Acquirer_Code =   @AcquirerCode
                                ORDER BY Rules_Code";
                var codes = await connectionContext.QueryAsync<int>(sql, new { AcquirerCode = acquirerCode });
                if (codes.Count() > 0)
                {
                    result = string.Join(",", codes);
                }
            }
            return result;
        }


        private async Task<string> GetMigrationUsedCodes()
        {
            string result = null;
            using (var connectionContext = dataContext.AcquireConnection())
            {
                string sql = @"SELECT Migration_Id FROM Rules_Migration";
                var codes = await connectionContext.QueryAsync<int>(sql, new { });
                if (codes.Count() > 0)
                {
                    result = string.Join(",", codes);
                }
            }
            return result;
        }

        public async Task<CampaignProdutcView[]> GetCampaignsByProduct(CampaignSearchByProductParameters parameters)
        {
            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            var campaigns = new List<CampaignProdutcView>();
            using(var connectionContext = campaignDataContext.AcquireConnection())
            {
                var sql = @"SELECT          
                                distinct rf.Rules_Code [Id]
                                ,RTRIM(rf.TktMsg_RulesInfo) + ' - ' + rf.Rules_Name [Name]
	                        FROM [TRNCENTRE_ADV_V7].[dbo].rules_msfile rf INNER JOIN
	                             [TRNCENTRE_ADV_V7].[dbo].rules_prd prod ON rf.Rules_Code = prod.Rules_Code AND rf.Rules_BeginDate = prod.Rules_BeginDate INNER JOIN
	                             [TRNCENTRE_ADV_V7].[dbo].Prd_MsFile prodInfo ON prod.Prd_StdCode = prodInfo.Prd_StdCode AND 
                                 prodInfo.Acquirer_Code = @AcquirerCode
	                        WHERE rf.Acquirer_Code = @AcquirerCode AND  
	                        rf.Rec_St ='A' AND
	                        (prod.Prd_StdCode = @EAN OR prodInfo.Prd_Brand = @BrandId )
                            ORDER BY [Name]";

                var result = await connectionContext.QueryAsync<CampaignProdutcView>(sql, new { 
                    AcquirerCode = parameters.AcquirerCode,
                    EAN = parameters.EAN,
                    BrandId = parameters.BrandId
                });

                if(result != null && result.Any())
                {
                    return result.ToArray();
                }
            }
         
            return campaigns.ToArray();
        }

        public async Task<Campaign> GetCampaignById(CampaignSearchParameters parameters)
        {
           
            Campaign campaign = null;
            
            using(var connectionContext = dataContext.AcquireConnection() )
            {
                var sql = @"SELECT
	                            Campaign.Rules_Code [Id]
	                            ,Campaign.Rules_BeginDate [BeginDate]
	                            ,Campaign.Rules_EndDate [EndDate]
	                            ,Campaign.Rules_LastDate [EndDateForPurchase]
	                            ,Campaign.Rules_Name [Name]
	                            ,Campaign.Acquirer_Code [AcquirerCode]
	                            ,Campaign.Rules_QtyMaxMember [MaximumNumberOfParticipants]
	                            ,Campaign.Acct_Nr [ProgramId]
	                            ,Campaign.Rules_LimitMode [LimitMode]
	                            ,Campaign.Rules_DescStateId [StateDiscountType]
	                            ,Campaign.Rules_BillNrDays [BillKitNumberOfDays]
	                            ,Campaign.Rules_ControlMode [ControlMode]
	                            ,Campaign.Rec_InsUser [UserName]
	                            ,Campaign.Prd_QtyLevelId [ThresholdControl]
                             FROM Rules_MsFile Campaign
                             WHERE
	                            Campaign.Acquirer_Code = @AcquirerCode AND
	                            Campaign.Rules_Code = @Id AND Campaign.Rec_St = 'A'";


                var search = (await connectionContext.QueryAsync<dynamic>(sql, new { AcquirerCode = Convert.ToInt32(parameters.AcquirerCode) ,
                Id = parameters.Id}));

                if (search != null && search.ToList().Count > 0)
                {
                    var result = search.ToList().First();
                    campaign = new Campaign();
                    campaign.Id = result.Id;
                    campaign.BeginDate = result.BeginDate;
                    campaign.EndDate = result.EndDate;
                    campaign.EndDateForPurchase = result.EndDateForPurchase;
                    campaign.Name = result.Name;
                    campaign.AcquirerCode = result.AcquirerCode;
                    campaign.MaximumNumberOfParticipants = result.MaximumNumberOfParticipants;
                    campaign.Program = new CompanyProgram() { Id = result.ProgramId };
                    campaign.LimitMode = result.LimitMode;
                    campaign.StateDiscountType = result.StateDiscountType;
                    campaign.BillKitNumberOfDays = result.BillKitNumberOfDays;
                    campaign.ControlMode = result.ControlMode;
                    campaign.ThresholdControl = result.ThresholdControl;
                    campaign.RecordInsertionUser = result.UserName;

                    sql = @"SELECT RTRIM(Active_PDV_Model) FROM Rules_AdherenceModel WHERE Acquirer_Code = @AcquirerCode AND
	                            Rules_Code = @Id AND Rec_St = 'A'";

                    var adherencesModels = (await connectionContext.QueryAsync<string>(sql, new
                    {
                        AcquirerCode = Convert.ToInt32(parameters.AcquirerCode),
                        Id = parameters.Id
                    }))?.ToList();

                    if(adherencesModels != null && adherencesModels.Count > 0)
                    {
                        var adherenceModelKey = string.Join(",", adherencesModels);
                        var adherenceModelMap = new Dictionary<string, string>() {
                            {"ATIVA2,ATIVA4","S" },
                            {"ATIVA1,ATIVA3,ATIVA5","SCRM" },
                            {"ATIVA4","C" },
                            {"ATIVA1,ATIVA5","CCRM" },
                            {"ATIVA6","SSG" },
                            {"ATIVA1,ATIVA7","CSE" }
                        };

                        campaign.AdhrenceModel = new CampaignAdherenceModel() { AdherenceDataType = adherenceModelMap[adherenceModelKey] };
                    }


                    campaign.Coupon = await couponRepository.GetDraftCouponByCampaign(new CouponSearchParameters(campaign.AcquirerCode,campaign.Id));

                    sql = @"SELECT 
                             Product.Rules_ComboId [AssociatedWithCombo]
                             ,Product.Prc_Desc [Discount]
                             ,Product.MultiPrd_Id [DiscountType]
                             ,Product.Prd_StdCode [EAN]
                             ,Product.[State]
                             ,Product.City_Code [City]
                             ,RTRIM([State].State_Name) [StateName]
                             ,Product.Prd_QtyMax [MaxQuantity]
                             FROM Rules_Prd Product INNER JOIN
                            Str_ZIP_State [State] ON Product.[State] = [State].[State]
                            WHERE Product.Rules_Code = @Id AND
                             Product.Rules_BeginDate = @BeginDate AND  Product.Rec_St = 'A'";

                    var productResult = await connectionContext.QueryAsync<dynamic>(sql, new { parameters.Id, campaign.BeginDate  });

                    if (productResult != null && productResult.ToList().Count() > 0)
                    {
                        var productKeys = new HashSet<string>();
                        var products = new List<Product>();

                        foreach(var p in productResult.ToList())
                        {
                            string EAN = p.EAN.ToString();
                            if(!productKeys.Contains(EAN))
                            {
                                productKeys.Add(EAN);

                                var product = new Product();

                                product.EAN = EAN;
                                string associatedWithCombo = p.AssociatedWithCombo;
                                product.AssociatedWithCombo = associatedWithCombo.Equals("Y") ? true : false;
                                product.Discount = p.Discount;
                                product.DiscountType = p.DiscountType.ToString()[0];
                                product.States = productResult.Where(s => s.EAN.ToString() == product.EAN).Select(state => new State()
                                {
                                    Abbreviation = state.State,
                                    Name = state.StateName,
                                    Cities = productResult.Where(c => c.EAN.ToString() == product.EAN && c.State == state.State).Select(city => new
                                    City()
                                    { Id = city.City }).ToArray()
                                }).Distinct(new StateComparer()).ToArray();

                                product.Brand = await productRepository.GetProductBrandByProduct(product.EAN);
                                product.Description = await productRepository.GetProductDescriptionByEAN(campaign.AcquirerCode, product.EAN);
                                var fullbox =  await productRepository.GetProductFullBox(new ProductSearchParameters(campaign.AcquirerCode, product.EAN, false));
                                var maxquantity = p.MaxQuantity * fullbox;
                                if (campaign.LimitMode > 0)
                                    maxquantity = maxquantity / campaign.LimitMode;

                                product.MaxQuantity = Convert.ToInt16(maxquantity);

                                if(product.States != null && product.States.Count() > 0)
                                {
                                    foreach(var s in product.States)
                                    {
                                        foreach(var city in s.Cities)
                                        {
                                            if (city.Id.Equals(0))
                                            {
                                                city.TotalQuantity = await cityRepository.GetTotalCityCountByState(s.Abbreviation);
                                            }
                                        }
                                    }
                                }

                                products.Add(product);
                            }

                        }

                        if(products.Count > 0)
                            campaign.Products = products.ToArray();
                    }


                    sql = @"SELECT PhysicianState.Profess_Code [ProfessionalCode]
                                  ,PhysicianState.Profess_Type [Type]
	                              ,PhysicianState.Profess_State [State]
	                         FROM Rules_Profess Physician INNER JOIN
                                  Rules_ProfessDetail PhysicianState 
                                  ON Physician.Rules_ProfessCode = PhysicianState.Rules_ProfessCode
                            WHERE  
	                            Physician.Acquirer_Code = @AcquirerCode AND
	                            Physician.Rules_ProfessCode = @Id AND
	                            Physician.Rules_ProfessBeginDate = @BeginDate AND Physician.Rec_St = 'A'";

                    var physicianResult = await connectionContext.QueryAsync<dynamic>(sql, new { AcquirerCode = campaign.AcquirerCode,
                    Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                     });

                    if(physicianResult != null && physicianResult.Count() > 0)
                    {
                        var physicianKeys = new HashSet<string>();
                        var physicians = new List<Physician>();
                        foreach(var physician in physicianResult)
                        {
                            var physicianKey = $"{physician.ProfessionalCode.ToString()} {physician.State.ToString()}";

                            if(!physicianKeys.Contains(physicianKey))
                            {
                                physicianKeys.Add(physicianKey);

                                physicians.Add(new Physician()
                                {
                                    CRM = Convert.ToInt32(physician.ProfessionalCode),
                                    ProfessionalType = physician.Type,
                                    State = new State()
                                    {
                                        Abbreviation = physician.State
                                    }
                                });
                            }
                        }

                        if(physicians.Count > 0)
                        {
                            campaign.Physicians = physicians.ToArray();
                        }
                    }

                    sql = @"SELECT AssociatedCampaign.Rules_CodeAppend [Id]
	                            ,AssociatedCampaign.Rules_Append_BeginDate [BeginDate]
	                            ,AssociatedCampaign.RulesAppend_EndTime [EndDate]
                             FROM Rules_Append  AssociatedCampaign
                             WHERE 
	                            AssociatedCampaign.Acquirer_Code = @AcquirerCode AND
	                            AssociatedCampaign.Rules_Code = @Id AND
	                            AssociatedCampaign.Rules_BeginDate = @BeginDate";

                    var associatecampaignResult = await connectionContext.QueryAsync<AssociatedCampaign> (sql, new {
                        AcquirerCode = campaign.AcquirerCode,
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(associatecampaignResult != null && associatecampaignResult.Count() > 0)
                    {
                        campaign.AssociatedCampaigns = associatecampaignResult.ToArray();
                    }

                    sql = @"SELECT Migration.Rules_CodeIn [CodeIn] 
                                FROM Rules_Migration Migration
                                WHERE 
	                                Migration.Migration_Id IN (
		                                SELECT Migration_Id FROM MigrationMapping WHERE Acquirer_Code = @AcquirerCode AND
		                                Rules_Code = @Id AND Rules_BeginDate = @BeginDate
	                                ) AND 
                                    Migration.Migration_Type = 'CMP'";

                    var migrationResult = await connectionContext.QueryAsync<CampaignMigration>(sql, new {
                        Id = campaign.Id,
                        AcquirerCode = campaign.AcquirerCode,
                        BeginDate = campaign.BeginDate
                    });

                    if(migrationResult != null && migrationResult.Count() >0)
                    {
                        campaign.MigratedCampaigns = migrationResult.ToArray();
                    }


                    sql = @"SELECT 
	                            AdditionalNetworkDiscount.Prd_StdCode [EAN]
	                            ,AdditionalNetworkDiscount.MsfCo_FlagGroup [FiscalNumberRoot]
	                            ,AdditionalNetworkDiscount.Prc_DescAd [Discount]
	                            ,AdditionalNetworkDiscount.[State]
                            FROM Rules_PrdFlag AdditionalNetworkDiscount
                            WHERE
	                            Rules_FlagBeginDate = @BeginDate AND
	                            Rules_FlagEndDate = @EndDate AND
	                            Rules_Code = @Id AND Rec_St = 'A'";

                    var additionalNetworkDiscountResult = await connectionContext.QueryAsync<dynamic>(sql, new
                    {
                        EndDate = campaign.EndDate,
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(additionalNetworkDiscountResult != null && additionalNetworkDiscountResult.Count() > 0)
                    {
                        var additionalNetworkDiscounts = new List<AdditionalNetworkDiscount>();
                        foreach(var additionalDiscount in additionalNetworkDiscountResult)
                        {
                            var networkName = (await networkRepository.GetNetworkByFiscalNumberRoot(new NetworkSearchParameters(campaign.AcquirerCode, additionalDiscount.FiscalNumberRoot.ToString())))?.Name;
                            var discount = new AdditionalNetworkDiscount(new Product()
                            {
                                EAN = additionalDiscount.EAN.ToString()
                            }, additionalDiscount.FiscalNumberRoot.ToString(),
                            new State() { Abbreviation = additionalDiscount.State.ToString() },Convert.ToDecimal(additionalDiscount.Discount.ToString()),
                            networkName
                            );
                            additionalNetworkDiscounts.Add(discount);
                           
                        }

                        campaign.AdditionalNetworkDiscounts = additionalNetworkDiscounts.ToArray();
                    }

                    sql = @"SELECT 
	                            Threshold.Prd_DescQtyBegin [InitialQuantity]
	                            ,Threshold.Prd_DescQtyEnd [FinalQuantity]
	                            ,Threshold.Prd_PeriodMax [PeriodMax]
	                            ,Threshold.Prd_Desc [Discount]
	                            ,Threshold.Prd_RemoveQty [RemoveQuantity]
                            FROM Rules_MultiPrd Threshold
                            WHERE 
	                            Threshold.Prd_RestartQty = '0' AND
                                Threshold.Rules_Code = @Id AND
	                            Threshold.Rules_BeginDate = @BeginDate AND  Threshold.Rec_St = 'A'";

                    var thresHoldResult = await connectionContext.QueryAsync<Threshold>(sql, new {
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(thresHoldResult != null && thresHoldResult.Count() > 0)
                    {
                        campaign.Thresholds = thresHoldResult.ToArray();
                    }


                    sql = @"SELECT 
	                            Threshold.Prd_DescQtyBegin [InitialQuantity]
	                            ,Threshold.Prd_DescQtyEnd [FinalQuantity]
	                            ,Threshold.Prd_PeriodMax [PeriodMax]
	                            ,Threshold.Prd_Desc [Discount]
	                            ,Threshold.Prd_RemoveQty [PunishmentDays]
                                ,Threshold.Prd_Desc_Penalty [ThresholdPunishmentAmount]
                            FROM Rules_MultiPrd Threshold
                            WHERE 
	                            Threshold.Prd_RestartQty = '99' AND
                                Threshold.Rules_Code = @Id AND
	                            Threshold.Rules_BeginDate = @BeginDate AND Threshold.Rec_St = 'A'";

                    var unitaryDiscountResult = await connectionContext.QueryAsync<ProgressiveDiscountByUnit>(sql, new
                    {
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if (unitaryDiscountResult != null && unitaryDiscountResult.Count() > 0)
                    {
                        campaign.UnitaryDiscounts = unitaryDiscountResult.ToArray();
                    }


                    sql = @"SELECT 
	                            MonthlyDiscount.Prd_DescQtyBegin [ProductInitialQuantity]
	                            ,MonthlyDiscount.Prd_DescQtyEnd [ProductFinalQuantity]
	                            ,MonthlyDiscount.Prd_Desc_Penalty [ThresholdPunishmentAmount]
	                            ,MonthlyDiscount.Prd_Desc [Discount]
	                            ,MonthlyDiscount.Prd_PeriodMax [MaximumPeriodForPunishment]
                            FROM Rules_MultiPrd_Month MonthlyDiscount
                            WHERE 
                                MonthlyDiscount.Rules_Code = @Id AND
	                            MonthlyDiscount.Rules_BeginDate = @BeginDate AND  MonthlyDiscount.Rec_St = 'A'";

                    var monthlyDiscountResult = await connectionContext.QueryAsync<MonthlyProgressiveDiscount>(sql, new { Id = campaign.Id,
                    BeginDate = campaign.BeginDate});

                    if(monthlyDiscountResult !=null && monthlyDiscountResult.Count() > 0)
                    {
                        campaign.MonthlyDiscounts = monthlyDiscountResult.ToArray();
                    }

                    sql = @"SELECT 
	                            ComboBrand.Prd_Brand1 [Brand1]
	                            ,ComboBrand.Prd_Brand2 [Brand2]
	                            ,ComboBrand.trn_BillQty1 [Quantity1]
	                            ,ComboBrand.trn_BillQty2 [Quantity2]
	                            ,ComboBrand.Prc_Desc1 [Discount1]
	                            ,ComboBrand.Prc_Desc2 [Discount2]
	                            ,ComboBrand.Rules_Code2 [CampaignId2]
                            FROM
	                            Rules_MixPrdBrand ComboBrand
                            WHERE 
	                            ComboBrand.Acquirer_Code = @AcquirerCode AND
	                            ComboBrand.Rules_MixBeginDate = @BeginDate AND
	                            ComboBrand.Rules_Code1 = @Id AND ComboBrand.Rec_St = 'A'";

                    var comboBrandResult = await connectionContext.QueryAsync<dynamic>(sql, new {
                        AcquirerCode = campaign.AcquirerCode,
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                   if(comboBrandResult != null && comboBrandResult.Count() > 0)
                    {
                        campaign.ComboBrand = comboBrandResult.Select(combo => new ComboBrandCampaign() {
                            Campaign2  = new Campaign() { Id = Convert.ToInt32(combo.CampaignId2) },
                            Product1 = new Product() {
                                Discount = combo.Discount1,
                                Brand = new Brand() { Id = Convert.ToInt32(combo.Brand1.ToString())} 
                            },
                            Product2 = new Product()
                            {
                                Discount = combo.Discount2,
                                Brand = new Brand() { Id = Convert.ToInt32(combo.Brand2.ToString()) }
                            },
                            Quantity1 = short.Parse(combo.Quantity1.ToString()),
                            Quantity2 = short.Parse(combo.Quantity2.ToString())

                        }).ToArray();
                    }

                    sql = @"SELECT 
	                            ComboEAN.Prd_StdCode1 [EAN1]
	                            ,ComboEAN.Prd_StdCode2 [EAN2]
	                            ,ComboEAN.trn_BillQty1 [Quantity1]
	                            ,ComboEAN.trn_BillQty2 [Quantity2]
	                            ,ComboEAN.Prc_Desc1 [Discount1]
	                            ,ComboEAN.Prc_Desc2 [Discount2]
	                            ,ComboEAN.Rules_Code2 [CampaignId2]
                            FROM
	                            Rules_MixPrdEAN ComboEAN
                            WHERE 
	                            ComboEAN.Acquirer_Code = @AcquirerCode AND
	                            ComboEAN.Rules_MixBeginDate = @BeginDate AND
	                            ComboEAN.Rules_Code1 = @Id AND ComboEAN.Rec_St = 'A'";

                    var comboEANResult = await connectionContext.QueryAsync<dynamic>(sql, new {
                        AcquirerCode = campaign.AcquirerCode,
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(comboEANResult != null && comboEANResult.Count() > 0)
                    {
                        campaign.Combos = comboEANResult.Select(combo => new ComboCampaign()
                        {
                            Campaign2 = new Campaign() { Id = Convert.ToInt32(combo.CampaignId2) },
                            Product1 = new Product()
                            {
                                Discount = combo.Discount1,
                                EAN = combo.EAN1.ToString()
                            },
                            Product2 = new Product()
                            {
                                Discount = combo.Discount2,
                                EAN = combo.EAN2.ToString()
                            },
                            Quantity1 = short.Parse(combo.Quantity1.ToString()),
                            Quantity2 = short.Parse(combo.Quantity2.ToString())

                        }).ToArray();
                    }

                    sql = @"SELECT 
                                Kit.[Prd_StdCode] as [EAN]
                                ,Kit.[Prd_NewKitQty] as [Quantity]
                                ,Kit.[Prd_NewKitDesc] as [Discount]
                            FROM [dbo].[Rules_PrdNewKit] Kit
                            WHERE Kit.Rules_Code = @Id AND
	                            Kit.Rules_BeginDate = @BeginDate AND  Kit.Rec_St = 'A'";

                    var kitResult = await connectionContext.QueryAsync<dynamic>(sql, new
                    {
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(kitResult != null && kitResult.Count() > 0)
                    {
                        campaign.Kits = kitResult.Select( kit => new CampaignKit() { 
                        
                            Discount = kit.Discount,
                            Quantity = short.Parse(kit.Quantity.ToString()),
                            Product = new Product() { EAN = kit.EAN.ToString() }
                        }).ToArray();
                    }

                    sql = @"SELECT 
	                            RTRIM(Channel.SystemType) [Name] 
                            FROM Rules_MsFile_Specific Channel
                            WHERE 
	                            Channel.Acquirer_Code = @AcquirerCode AND
	                            Channel.Rules_Code = @Id AND
	                            Channel.Rules_BeginDate = @BeginDate AND
	                            RTRIM(Channel.Estab_Chain) = '' AND  Channel.Rec_St = 'A'";

                    var channelResult = await connectionContext.QueryAsync<MembershipChannel>(sql, new {
                        AcquirerCode = campaign.AcquirerCode,
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(channelResult != null && channelResult.Count() > 0)
                    {
                        campaign.Channels = channelResult.ToArray();
                    }


                    sql = @"SELECT 
	                            RTRIM(Network.Estab_Chain) [FiscalNumberRoot] 
                            FROM Rules_MsFile_Specific Network
                            WHERE 
	                            Network.Acquirer_Code = @AcquirerCode AND
	                            Network.Rules_Code = @Id AND
	                            Network.Rules_BeginDate = @BeginDate AND
	                            RTRIM(Network.SystemType) = '' AND Network.Rec_St = 'A'";

                    var networkResult = await connectionContext.QueryAsync<Network>(sql, new
                    {
                        AcquirerCode = campaign.AcquirerCode,
                        Id = campaign.Id,
                        BeginDate = campaign.BeginDate
                    });

                    if(networkResult != null && networkResult.Count() > 0)
                    {
                        foreach(var network in networkResult)
                        {
                            network.Name = (await networkRepository.GetNetworkByFiscalNumberRoot(new NetworkSearchParameters(campaign.AcquirerCode, network.FiscalNumberRoot))).Name;
                        }
                        campaign.Networks = networkResult.ToArray();
                    }


                    sql = @"SELECT

                                Question.Question_Code[Id]
	                            ,Question.Prd_Brand[BrandId]
	                            ,Question.Questions_Text[Text]
                            FROM Pub_Str_Holder_Questions Question INNER JOIN
                            QuestionnaireMapping QuestionCampaignMap
                            ON Question.Question_Code = QuestionCampaignMap.Question_Code
                            WHERE

                                Question.Acquirer_Code = @AcquirerCode AND

                                Question.Rec_St = 'A' AND
                                QuestionCampaignMap.Rules_Code = @RulesCode AND

                                QuestionCampaignMap.Acquirer_Code = @AcquirerCode";
                    var questionResult = await connectionContext.QueryAsync<dynamic>(sql, new { AcquirerCode = campaign.AcquirerCode,
                        RulesCode = campaign.Id
                    });

                    if(questionResult != null && questionResult.Count() > 0)
                    {
                        var questionnaire = new Questionnaire();
                        questionnaire.Brand = await productRepository.GetBrandById(Convert.ToInt32(questionResult.First().BrandId.ToString()));

                        sql = @"SELECT 
	                            CONVERT(int,QuestionnaireRule.Table_KeyCode2) [From]
	                            ,CONVERT(int,QuestionnaireRule.Table_KeyCode3) [To]
	                            ,CONVERT(int,QuestionnaireRule.Table_Info1) [AssociatedWith]
                            FROM Str_Multi_Table QuestionnaireRule
                            WHERE 
	                            QuestionnaireRule.Acquirer_Code = @AcquirerCode AND 
	                            QuestionnaireRule.Table_KeyType = 'class_rules_code' AND
	                            [Table_KeyCode] = CONVERT(char(20),@Id) AND QuestionnaireRule.Rec_St = 'A' ";

                        var quetionnaireRulesResult = await connectionContext.QueryAsync<dynamic>(sql, new {
                            AcquirerCode = campaign.AcquirerCode,
                            Id = campaign.Id
                        });

                        if(quetionnaireRulesResult != null && quetionnaireRulesResult.Count() > 0)
                        {
                            questionnaire.QuestionnaireRules = quetionnaireRulesResult.Select(rule => new QuestionnaireCampaignRule() { 
                                AssociateWIth = new Campaign() { Id = Convert.ToInt32(rule.AssociatedWith) },
                                From = Convert.ToInt32(rule.From),
                                To = Convert.ToInt32(rule.To)
                            }).ToArray();
                        }

                        sql = @"SELECT 
	                                Answer.Answer_Code [Id]
	                                ,RTRIM(Answer.Answer_Text) [Text]
	                                ,Answer.Answer_AllowFreeText [AllowFreeText]
	                                ,ISNULL(AnswerDefault.Answer_Code,-1) [Default]
	                                ,Answer.Question_Code
                                    ,ISNULL(QuestionWeight.Point,0) [Weight]
                                FROM Pub_Str_Holder_Answers Answer LEFT JOIN 
	                                 Pub_Str_Holder_AnswersDefault AnswerDefault
	                                 ON Answer.Answer_Code = AnswerDefault.Answer_Code AND
	                                 Answer.Question_Code = AnswerDefault.Question_Code  LEFT JOIN
                                     Str_Social_Economic QuestionWeight ON
                                     Answer.Question_Code = QuestionWeight.Question_Code AND
                                     Answer.Answer_Code = QuestionWeight.Answers_Code AND
                                     QuestionWeight.Acquirer_Code =  @AcquirerCode
                                WHERE 
	                                Answer.Question_Code = @Id AND Answer.Rec_St = 'A' ";

                        var questions = new List<Question>();
                        foreach (var question in questionResult)
                        {
                            var q = new Question();
                            q.Id = Convert.ToInt32(question.Id);
                            q.Text = question.Text.ToString();


                            var answerResult = await connectionContext.QueryAsync<dynamic>(sql, new { Id = q.Id, AcquirerCode = campaign.AcquirerCode });

                            if(answerResult != null && answerResult.Count() > 0)
                            {
                                q.Answers = answerResult.Select(answer => new Answer() { 
                                    Id = Convert.ToInt32(answer.Id),
                                    AllowFreeText = answer.AllowFreeText.ToString().Equals("Y")?true:false,
                                    Text = answer.Text.ToString(),
                                    Default = Convert.ToInt32(answer.Default) > -1? true:false ,
                                    Weight = answer.Weight
                                }).ToArray();
                            }

                            questions.Add(q);

                        }

                        questionnaire.Questions = questions.ToArray();

                        campaign.Questionnaire = questionnaire;
                    }               
                }
            }



            return campaign;
        }

        public async Task<bool> DeactivateCampaign(CampaignDeactivationParameters parameters)
        {
            var messageParameters = new Dictionary<string, object>();

            messageParameters.Add("FUN", "IRC_OnOffCampanha");
            messageParameters.Add("ACQ", parameters.AcquirerCode);
            messageParameters.Add("RLC", parameters.Id);
            messageParameters.Add("RBD", parameters.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("RON", 0);

            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
            var authorizerRepository = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateConnection();

            var result = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
            var xmlResult = result.Value;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlResult);
            XmlNodeList xnList = xmlDocument.SelectNodes("/ROOT/TRA");
            if (xnList != null && xnList.Count > 0 && Convert.ToInt32(xnList[0].InnerText) > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ActivateCampaign(CampaignDeactivationParameters parameters)
        {
            var messageParameters = new Dictionary<string, object>();

            messageParameters.Add("FUN", "IRC_OnOffCampanha");
            messageParameters.Add("ACQ", parameters.AcquirerCode);
            messageParameters.Add("RLC", parameters.Id);
            messageParameters.Add("RBD", parameters.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("RON", 1);

            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
            var authorizerRepository = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateConnection();

            var result = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
            var xmlResult = result.Value;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlResult);
            XmlNodeList xnList = xmlDocument.SelectNodes("/ROOT/TRA");
            if (xnList != null && xnList.Count > 0 && Convert.ToInt32(xnList[0].InnerText) > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<int> EditCampaign(Campaign campaign)
        {
            var campaignId = 0;
            var messageParameters = new Dictionary<string, object>();

            messageParameters.Add("FUN", "IRC_EditarCampanha");
            messageParameters.Add("ACQ", campaign.AcquirerCode);
            messageParameters.Add("RLC", campaign.Id);
            messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("RED", campaign.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("RLD", campaign.EndDateForPurchase.ToString("yyyy-MM-dd HH:mm:ss"));
            messageParameters.Add("USC", campaign.RecordInsertionUser);

            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
            var authorizerRepository = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateConnection();

            var result = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
            var xmlResult = result.Value;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlResult);
            XmlNodeList xnList = xmlDocument.SelectNodes("/ROOT/ID");
            if (xnList != null && xnList.Count > 0 && Convert.ToInt32(xnList[0].InnerText) > 0)
            {
                campaignId = Convert.ToInt32(xnList[0].InnerText);
            }
            return campaignId;


        }

        public async Task<Campaign> GetCampaignBasicInformationById(ActiveAndExpiredCampaignSearchParameters parameters)
        {
            var campaignDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();
            using(var connectionContext = campaignDataContext.AcquireConnection())
            {
                var sql = @"SELECT
	                            Campaign.Rules_Code [Id]
	                            ,Campaign.Rules_BeginDate [BeginDate]
	                            ,Campaign.Rules_EndDate [EndDate]
	                            ,Campaign.Rules_LastDate [EndDateForPurchase]
	                            ,Campaign.Rules_Name [Name]
	                            ,Campaign.Acquirer_Code [AcquirerCode]
	                            ,Campaign.Rules_QtyMaxMember [MaximumNumberOfParticipants]
	                            ,Campaign.Acct_Nr [ProgramId]
	                            ,Campaign.Rules_LimitMode [LimitMode]
	                            ,Campaign.Rules_DescStateId [StateDiscountType]
	                            ,Campaign.Rules_BillNrDays [BillKitNumberOfDays]
	                            ,Campaign.Rules_ControlMode [ControlMode]
	                            ,Campaign.Prd_QtyLevelId [ThresholdControl]
								,Campaign.Rec_InsUser [User]
								,RTRIM(Adhesion.Active_PDV_Model) [ADM]
                             FROM Trncentre_ADV_V7.dbo.Rules_MsFile Campaign LEFT JOIN 
							 Trncentre_ADV_V7.dbo.Rules_AdherenceModel Adhesion ON
							 Campaign.Acquirer_Code = Adhesion.Acquirer_Code AND
							 Campaign.Rules_Code = Adhesion.Rules_Code
                             WHERE
	                            Campaign.Acquirer_Code = @AcquirerCode AND
	                            Campaign.Rules_Code = @Id AND
								Campaign.Rec_St IN ('A','D')";

                var result = await connectionContext.QueryAsync<dynamic>(sql, new { AcquirerCode = parameters.AcquirerCode, Id = parameters.Id });
                if(result != null && result.Any())
                {
                    var campaign = result.ToList().Select(c => new Campaign()
                    {
                        Id = Convert.ToInt32(c.Id.ToString()),
                        Name = c.Name.ToString(),
                        BeginDate = Convert.ToDateTime(c.BeginDate.ToString()),
                        EndDate = Convert.ToDateTime(c.EndDate.ToString()),
                        EndDateForPurchase = Convert.ToDateTime(c.EndDateForPurchase.ToString()),
                        AcquirerCode = parameters.AcquirerCode,
                        MaximumNumberOfParticipants = Convert.ToInt32(c.MaximumNumberOfParticipants.ToString()),
                        Program = new CompanyProgram() { Id = Convert.ToInt32(c.ProgramId.ToString()) },
                        LimitMode = short.Parse(c.LimitMode.ToString()),
                        StateDiscountType = short.Parse(c.StateDiscountType.ToString()),
                        BillKitNumberOfDays = short.Parse(c.BillKitNumberOfDays.ToString()),
                        ControlMode = short.Parse(c.ControlMode.ToString()),
                        ThresholdControl = c.ThresholdControl.ToString(),
                        RecordInsertionUser = c.User.ToString()

                    }).ToList()[0];

                    if (result.ToList()[0].ADM != null && !string.IsNullOrEmpty(result.ToList()[0].ADM.ToString()))
                    {
                        var adherenceModels = result.ToList().Select(x => x.ADM.ToString());
                        var adherenceModelKey = string.Join(",", adherenceModels);
                        var adherenceModelMap = new Dictionary<string, string>() {
                            {"ATIVA2,ATIVA4","S" },
                            {"ATIVA1,ATIVA3,ATIVA5","SCRM" },
                            {"ATIVA4","C" },
                            {"ATIVA1,ATIVA5","CCRM" },
                            {"ATIVA6","SSG" },
                            {"ATIVA1,ATIVA7","CSE" }
                        };
                        campaign.AdhrenceModel = new CampaignAdherenceModel() { AdherenceDataType = adherenceModelMap[adherenceModelKey] };
                    }

                    if (campaign != null)
                    {
                        campaign.Products = await productRepository.GetProductsByCampaign(campaign);
                    }

                    return campaign;
                }
            }
    

            return null;
        }

        public async Task<bool> DraftCampaignExists(int id, int acquirerCode)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sql = @"SELECT 
                                 COUNT(*) 
                            FROM Rules_MsFile 
                            WHERE Acquirer_Code = @AcquirerCode AND Rules_Code = @Id";
                var result = (await connectionContext.QueryAsync<int>(sql, new { AcquirerCode = acquirerCode,
                Id = id })).First();

                return result > 0;
            }
        }
    }
 }
