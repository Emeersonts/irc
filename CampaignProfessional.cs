namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignProfessional
    {
        public Campaign Campaign { get; set; }
        public Brand Brand { get; set; }
        public Physician[] Physicians { get; set; }
    }
}
