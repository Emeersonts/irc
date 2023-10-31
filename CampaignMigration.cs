namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignMigration
    {
        public int Id { get; set; }
        public int CodeIn { get; set; }
        public Campaign Campaign { get; set; }
        public string Type { get; set; }
    }
}
