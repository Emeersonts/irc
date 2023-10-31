using IDP.Authorizer;
using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly IDataContext dataContext;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;

        public QuestionnaireRepository(IDataContext dataContext, ICampaignConfigurationFactory campaignConfigurationFactory)
        {
            this.dataContext = dataContext;
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async  Task<bool> CheckQuestionnaireCanBeSaved(int acquirerCode, int brandId)
        {
            var previousCodes = await GetUsedBrands(acquirerCode);
            var brands = new List<Brand>();
            var productDataContext = campaignConfigurationFactory.Create(CampaignEnvironmentType.Production).CreateDataContext();

            using(var connectionContext = productDataContext.AcquireConnection())
            {
                var sql = @"SELECT DISTINCT Question.Prd_Brand [Id]
                              FROM Pub_Str_Holder_Questions Question
		                      WHERE 
			                    Question.Acquirer_Code = @AcquirerCode AND
			                    Question.Prd_Brand NOT IN (
				                    SELECT vl_camp FROM [dbo].[fn_split] (
												                       @PreviousCodes + ',0'
												                      ,','
			                    ))";

                var result = await connectionContext.QueryAsync<Brand>(sql, new {AcquirerCode = acquirerCode, PreviousCodes = previousCodes });

                if(result != null && result.Any())
                {
                    var auxbrands = result.ToList();
                    if (auxbrands != null && auxbrands.Any())
                    {
                        brands.AddRange(auxbrands.ToArray());
                    }
                    if (previousCodes != null)
                    {
                        brands.AddRange(previousCodes.Split(',').Select(code => new Brand() { Id = Convert.ToInt32(code) }));
                    }
                    if (brands.Find(b => b.Id == brandId) != null)
                    {
                        return false;
                    }
                }
            }
           
            return true;
        }

        public async Task<int> SaveQuestionnaire(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog)
        {
            var recordsAffected = 0;
            var sqlQuestion = @" DECLARE @QuestionCode INT
                                if(NOT EXISTS(SELECT Question_Code FROM Pub_Str_Holder_Questions WHERE Acquirer_Code = @Acquirer_Code AND
	                                    Prd_Brand = @Prd_Brand AND Question_Orderly = @Question_Orderly ))
                                BEGIN
                                INSERT INTO [dbo].[Pub_Str_Holder_Questions]
                                    ([Acquirer_Code]
                                    ,[Portal_Code]
                                    ,[Prd_Brand]
                                    ,[Questions_Text]
                                    ,[Question_BeginDate]
                                    ,[Question_EndDate]
                                    ,[Question_Orderly]
                                    ,[Rec_St]
                                    ,[Rec_StInfo]
                                    ,[Rec_InsTime]
                                    ,[Rec_InsUser]
                                    ,[Rec_UpdTime]
                                    ,[Rec_UpdUser])
                                VALUES
                                    (
		                            @Acquirer_Code
                                    ,@Portal_Code
                                    ,@Prd_Brand
                                    ,@Questions_Text
                                    ,@Question_BeginDate
                                    ,@Question_EndDate
                                    ,@Question_Orderly
                                    ,@Rec_St
                                    ,@Rec_StInfo
                                    ,@Rec_InsTime
                                    ,@Rec_InsUser
                                    ,@Rec_UpdTime
                                    ,@Rec_UpdUser
		                            ) 
	                                SELECT @QuestionCode = @@identity
                                END
                                ELSE
                                    BEGIN
                                        UPDATE [dbo].[Pub_Str_Holder_Questions]
                                        SET [Questions_Text] = @Questions_Text
                                            ,[Question_BeginDate] = @Question_BeginDate
                                            ,[Question_EndDate] = @Question_EndDate
                                            ,[Rec_UpdTime] = @Rec_UpdTime
                                            ,[Rec_UpdUser] = @Rec_UpdUser
                                            ,[Rec_St] = 'A'
                                            WHERE Acquirer_Code = @Acquirer_Code AND
											Prd_Brand = @Prd_Brand AND Question_Orderly = @Question_Orderly
		                                    SELECT @QuestionCode = (SELECT Question_Code FROM Pub_Str_Holder_Questions WHERE Acquirer_Code = @Acquirer_Code AND
								                                Prd_Brand = @Prd_Brand AND Question_Orderly = @Question_Orderly)
                                        END
                                    select @QuestionCode";


            var sqlAnswer = @"INSERT INTO [dbo].[Pub_Str_Holder_Answers]
                                    (
                                    [Question_Code]                    
                                    ,[Answer_Code]
                                    ,[Answer_Text]
                                    ,[Answer_AllowFreeText]
                                    ,[Answer_Orderly]
                                    ,[Rec_St]
                                    ,[Rec_StInfo]
                                    ,[Rec_InsTime]
                                    ,[Rec_InsUser]
                                    ,[Rec_UpdTime]
                                    ,[Rec_UpdUser])
                                VALUES
                                    (@Question_Code
                                    ,@Answer_Code
                                    ,@Answer_Text
                                    ,@Answer_AllowFreeText
                                    ,@Answer_Orderly
                                    ,@Rec_St
                                    ,@Rec_StInfo
                                    ,@Rec_InsTime
                                    ,@Rec_InsUser
                                    ,@Rec_UpdTime
                                    ,@Rec_UpdUser
		                            )
                                SELECT @@ROWCOUNT";

            var sqlAnswerDefault = @"INSERT INTO [dbo].[Pub_Str_Holder_AnswersDefault]
                                            ([Question_Code]
                                            ,[Answer_Code]
                                            ,[Rules_Begindate]
                                            ,[Rec_St]
                                            ,[Rec_StInfo]
                                            ,[Rec_InsTime]
                                            ,[Rec_InsUser]
                                            ,[Rec_UpdTime]
                                            ,[Rec_UpdUser])
                                        VALUES
                                            (
		                                    @Question_Code
                                            ,@Answer_Code
                                            ,@Rules_Begindate
                                            ,@Rec_St
                                            ,@Rec_StInfo
                                            ,@Rec_InsTime
                                            ,@Rec_InsUser
                                            ,@Rec_UpdTime
                                            ,@Rec_UpdUser
		                                    )
                                        SELECT @@ROWCOUNT";

        var sqlInsertAnswerWeight = @"INSERT INTO [dbo].[Str_Social_Economic]
                    ([Acquirer_Code]
                    ,[Question_Code]
                    ,[Answers_Code]
                    ,[Point]
                    ,[Rec_St]
                    ,[Rec_StInfo]
                    ,[Rec_InsTime]
                    ,[Rec_InsUser]
                    ,[Rec_UpdTime]
                    ,[Rec_UpdUser])
                VALUES
                    (@Acquirer_Code,
                    @Question_Code,
                    @Answers_Code,
                    @Point,
                    @Rec_St,
                    @Rec_StInfo,
                    @Rec_InsTime,
                    @Rec_InsUser,
                    @Rec_UpdTime,
                    @Rec_UpdUser
		            )
		        select @@rowcount";

        var sqlInsertQuestionnaireMapping = @"INSERT INTO [dbo].[QuestionnaireMapping]
                                                        ([Rules_Code]
                                                        ,[Acquirer_Code]
                                                        ,[Question_Code])
                                                    VALUES
                                                        (@Rules_Code
                                                        ,@Acquirer_Code
                                                        ,@Question_Code)";

            var authorizerRepository = campaignConfigurationFactory.Create(environmentType).CreateConnection();

            using (var connectionContext = dataContext.AcquireConnection())
            {
                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Pub_Str_Holder_AnswersDefault] WHERE Question_Code IN (
	                        SELECT Question_Code FROM [dbo].[QuestionnaireMapping] WHERE Acquirer_Code = @AcquirerCode AND Rules_Code = @Id)", new
                {
                    AcquirerCode = campaign.AcquirerCode,
                    Id = campaign.Id
                });

                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Pub_Str_Holder_Answers] WHERE Question_Code IN (
	                        SELECT Question_Code FROM [dbo].[QuestionnaireMapping] WHERE Acquirer_Code = @AcquirerCode AND Rules_Code = @Id)", new
                {
                    AcquirerCode = campaign.AcquirerCode,
                    Id = campaign.Id
                });

                var sqlDeleteQuestion = @"DELETE FROM Pub_Str_Holder_Questions WHERE Question_Code IN (
	                        SELECT Question_Code FROM [dbo].[QuestionnaireMapping] WHERE Acquirer_Code = @AcquirerCode AND Rules_Code = @Id)";

                var sqlUpdateQuestion = @"UPDATE Pub_Str_Holder_Questions SET Rec_ST = 'D' WHERE Question_Code IN (
	                        SELECT Question_Code FROM [dbo].[QuestionnaireMapping] WHERE Acquirer_Code = @AcquirerCode AND Rules_Code = @Id)";

                await connectionContext.ExecuteAsync(environmentType.Equals(CampaignEnvironmentType.Production)? sqlDeleteQuestion: sqlUpdateQuestion, new
                {
                    AcquirerCode = campaign.AcquirerCode,
                    Id = campaign.Id
                });

                await connectionContext.ExecuteAsync(@"DELETE FROM QuestionnaireMapping WHERE dbo.QuestionnaireMapping.Rules_Code = @RulesCode 
                        AND dbo.QuestionnaireMapping.Acquirer_Code = @AcquirerCode",
                    new { AcquirerCode = campaign.AcquirerCode, RulesCode = campaign.Id });

                await connectionContext.ExecuteAsync(@"DELETE FROM [dbo].[Str_Multi_Table] WHERE 
	                                                    Table_KeyType = 'class_rules_code' AND
	                                                    [Table_KeyCode] = CONVERT(char(20),@Id) AND
	                                                    Acquirer_Code = @AcquirerCode",
                        new
                        {
                            Id = campaign.Id,
                            AcquirerCode = campaign.AcquirerCode
                        });

            if ( campaign.Questionnaire != null && campaign.Questionnaire.QuestionnaireRules != null && campaign.Questionnaire.QuestionnaireRules.Length > 0)
                {
                    var sqlInsertQuestionnairRules = @"
	                                            INSERT INTO [dbo].[Str_Multi_Table]
			                                                ([Table_KeyType]
			                                                ,[Table_KeyCode]
			                                                ,[Table_KeyCode2]
			                                                ,[Table_KeyCode3]
			                                                ,[Acquirer_Code]
			                                                ,[Table_Info1]
			                                                ,[Table_Info2]
			                                                ,[Table_Info3]
			                                                ,[Table_Info4]
			                                                ,[Rec_St]
			                                                ,[Rec_UpdTime]
			                                                ,[Rec_UpdUser])
		                                                VALUES
			                                                ('class_rules_code'
			                                                ,@Id
			                                                ,@From
			                                                ,@To
			                                                ,@AcquirerCode
			                                                ,@AssociatedWith
			                                                ,''
			                                                ,''
			                                                ,''
			                                                ,'A'
			                                                ,GETDATE()
			                                                ,@UserName
			                                                )
                                                    select @@rowcount";

                    foreach (var rule in campaign.Questionnaire.QuestionnaireRules)
                    {
                        var messageParameters = new Dictionary<string, object>();
                        messageParameters.Add("FUN", "IRC_InsRQuestionario");
                        messageParameters.Add("RLC", campaign.Id);
                        messageParameters.Add("QIV", rule.From);
                        messageParameters.Add("QFV", rule.To);
                        messageParameters.Add("RL2", rule.AssociateWIth.Id);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);
                        messageParameters.Add("USC", campaign.RecordInsertionUser);

                        var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));

                        await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            await connectionContext.QueryAsync<int>(sqlInsertQuestionnairRules, new
                            {
                                Id = campaign.Id,
                                From = rule.From,
                                To = rule.To,
                                AssociatedWith = rule.AssociateWIth.Id,
                                UserName = campaign.RecordInsertionUser,
                                AcquirerCode = campaign.AcquirerCode
                            });
                        }
                    }

                var devQuestionCode = 0;

                foreach (var question in campaign.Questionnaire.Questions)
                {
                    var messageParameters = new Dictionary<string, object>();
                    messageParameters.Add("FUN", "IRC_InserirQuestao");
                    messageParameters.Add("ACQ", campaign.AcquirerCode);
                    messageParameters.Add("PRB", campaign.Questionnaire.Brand.Id);
                    messageParameters.Add("QST", question.Text);
                    messageParameters.Add("QBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    messageParameters.Add("QED", campaign.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    messageParameters.Add("QSO", campaign.Questionnaire.Questions.ToList().IndexOf(question) + 1);
                    messageParameters.Add("USC", campaign.RecordInsertionUser);
                    messageParameters.Add("RLC", campaign.Id);

                    var authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                    var result = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                    if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                    {
                        devQuestionCode = (await connectionContext.QueryAsync<int>(sqlQuestion, new QuestionEntity()
                        {
                            Acquirer_Code = campaign.AcquirerCode,
                            Portal_Code = string.Empty,
                            Prd_Brand = campaign.Questionnaire.Brand.Id.ToString(),
                            Questions_Text = question.Text,
                            Question_BeginDate = campaign.BeginDate,
                            Question_EndDate = campaign.EndDate,
                            Question_Orderly = campaign.Questionnaire.Questions.ToList().IndexOf(question) + 1,
                            Rec_InsTime = DateTime.Now,
                            Rec_InsUser = campaign.RecordInsertionUser,
                            Rec_St = 'A',
                            Rec_StInfo = string.Empty,
                            Rec_UpdTime = DateTime.Now,
                            Rec_UpdUser = campaign.RecordInsertionUser
                        })).ToList().First();
                        await connectionContext.ExecuteAsync(sqlInsertQuestionnaireMapping, new
                        {
                            Rules_Code = campaign.Id,
                            Acquirer_Code = campaign.AcquirerCode,
                            Question_Code = devQuestionCode
                        });
                    }
                           
                    var xmlResult = result.Value;
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xmlResult);
                    XmlNodeList xnList = xmlDocument.SelectNodes("/ROOT/QSC");
                    var questionCode = Convert.ToInt32(xnList[0].InnerText);
                    await connectionContext.ExecuteAsync(@"DELETE FROM STR_SOCIAL_ECONOMIC WHERE Acquirer_Code = @AcquirerCode AND Question_Code = @QuestionCode",
                    new { AcquirerCode = campaign.AcquirerCode, questionCode = devQuestionCode });
                    foreach (var answer in question.Answers)
                    {
                        messageParameters.Clear();
                        messageParameters.Add("FUN", "IRC_INSResposta");
                        messageParameters.Add("AFT", answer.AllowFreeText ? "Y" : "N");
                        messageParameters.Add("AOR", question.Answers.ToList().IndexOf(answer) + 1);
                        messageParameters.Add("ARC", question.Answers.ToList().IndexOf(answer) + 1);
                        messageParameters.Add("ART", answer.Text);
                        messageParameters.Add("QTC", questionCode);
                        messageParameters.Add("USC", campaign.RecordInsertionUser);
                        messageParameters.Add("ANW", answer.Weight);
                        messageParameters.Add("ACQ", campaign.AcquirerCode);
                        authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                        var testeResult2 = await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());

                        if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                        {
                            recordsAffected += (await connectionContext.QueryAsync<int>(sqlAnswer, new AnswerEntity()
                            {
                                Answer_AllowFreeText = answer.AllowFreeText ? 'Y' : 'N',
                                Answer_Orderly = question.Answers.ToList().IndexOf(answer) + 1,
                                Answer_Code = question.Answers.ToList().IndexOf(answer) + 1,
                                Answer_Text = answer.Text,
                                Question_Code = devQuestionCode,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser

                            })).ToList().First();
                            await connectionContext.QueryAsync<int>(sqlInsertAnswerWeight, new AnswerWeitghtEntity()
                            {
                                Acquirer_Code = campaign.AcquirerCode,
                                Answers_Code = question.Answers.ToList().IndexOf(answer) + 1,
                                Point = answer.Weight,
                                Question_Code = devQuestionCode,
                                Rec_InsTime = DateTime.Now,
                                Rec_InsUser = campaign.RecordInsertionUser,
                                Rec_St = 'A',
                                Rec_StInfo = string.Empty,
                                Rec_UpdTime = DateTime.Now,
                                Rec_UpdUser = campaign.RecordInsertionUser
                            });
                        }

                        if (answer.Default)
                        {
                            messageParameters.Clear();
                            messageParameters.Add("FUN", "IRC_INSRespostaD");
                            messageParameters.Add("ARC", question.Answers.ToList().IndexOf(answer) + 1);
                            messageParameters.Add("QTC", questionCode);
                            messageParameters.Add("RBD", campaign.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            messageParameters.Add("USC", campaign.RecordInsertionUser);
                            authorizerMessage = new AuthorizerMessage(AuthorizerMessageBuilder.BuildDefaultMessage(messageParameters));
                            await authorizerRepository.ExecuteFunctionAsync(authorizerMessage, Autorizer.Logix());
                           
                            if (environmentType.Equals(CampaignEnvironmentType.Homolog))
                            {
                                recordsAffected += (await connectionContext.QueryAsync<int>(sqlAnswerDefault, new AnswerDefaultEntity()
                                {
                                    Answer_Code = question.Answers.ToList().IndexOf(answer) + 1,
                                    Question_Code = devQuestionCode,
                                    Rec_InsTime = DateTime.Now,
                                    Rec_InsUser = campaign.RecordInsertionUser,
                                    Rec_St = 'A',
                                    Rec_StInfo = string.Empty,
                                    Rec_UpdTime = DateTime.Now,
                                    Rec_UpdUser = campaign.RecordInsertionUser,
                                    Rules_Begindate = campaign.BeginDate
                                })).ToList().First();
                            }
                        }

                    }
                }
              }
            }

            return recordsAffected;
        }

        private async Task<string> GetUsedBrands(int acquirerCode)
        {
            string result = null;
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sql = @"SELECT DISTINCT CONVERT(INT,Prd_Brand) 
                               FROM Pub_Str_Holder_Questions 
                               WHERE Acquirer_Code = @AcquirerCode";
                var codes = (await connectionContext.QueryAsync<int>(sql, new { AcquirerCode = acquirerCode })).ToArray();
                if (codes.Length > 0)
                {
                    result = string.Join(",", codes);
                }

               
            }
            return result;
        }
    }
}
