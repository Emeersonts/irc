namespace BackOffice.Authorizer.Management.Domains
{
    public class Physician
    {
        public int CRM { get; set; }
        public short LimitMonth { get; set; }
        public short CountMonth { get; set; }
        public string ProfessionalType { get; set; }
        public State State { get; set; }
    }
}
