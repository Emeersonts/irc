namespace BackOffice.Authorizer.Management.Domains
{
    public class Questionnaire
    {
        public Question[] Questions { get; set; }

        public Brand Brand { get; set; }

        public QuestionnaireCampaignRule[] QuestionnaireRules { get; set; }
    }
}
