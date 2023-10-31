namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignConfiguration
    {
        public CampaignEnvironmentType Type { get; private set; }
        public string Connection { get; private set; }

        public CampaignConfiguration(CampaignEnvironmentType type, string connection)
        {
            Type = type;
            Connection = connection;
        }
    }
}