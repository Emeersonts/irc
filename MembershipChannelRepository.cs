using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class MembershipChannelRepository : IMembershipChannelRepository
    {
        private readonly IDataContext dataContext;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;

        public MembershipChannelRepository(IAuthorizerRepository authorizerRepository, IDataContext dataContext, ICampaignConfigurationFactory campaignConfigurationFactory)
        {
            this.dataContext = dataContext;
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async Task<int> SaveChannels(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
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
			                     AND Rules_BeginDate = Rules_BeginDate AND  RTRIM(SystemType) <> ''", new
                {
                    Rules_BeginDate = campaign.BeginDate,
                    Rules_Code = campaign.Id
                });

                if (campaign.Channels != null && campaign.Channels.Count() > 0)
                {
                    foreach (var channel in campaign.Channels)
                    {
                        if(environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {

                            Dictionary<string, object> messageParameters = new Dictionary<string, object>();
                            messageParameters.Add("FUN", "IRC_InserirRedes");
                            messageParameters.Add("RLC", campaign.Id);
                            messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            messageParameters.Add("USC", campaign.RecordInsertionUser);
                            messageParameters.Add("FNR", new string(' ', 20));
                            messageParameters.Add("CHR", channel.Name);
                            messageParameters.Add("ACQ", campaign.AcquirerCode);

                            var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                            var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();
                            var insertResult = authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                            recordsAffected += (await connectionContext.QueryAsync<int>(sql, new RulesSpecificEntity()
                            {

                                Estab_Chain = new string(' ', 20),
                                Acquirer_Code = campaign.AcquirerCode,
                                SystemType = channel.Name,
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
                    }
                }

            }

            return recordsAffected;
        }
    }
}
