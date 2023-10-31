namespace BackOffice.Authorizer.Management.Domains
{
    public class QuestionnaireCampaignRule
    {
        public int From { get; set; }

        public int To { get; set; }

        public Campaign AssociateWIth { get; set; }
    }
}
