namespace BackOffice.Authorizer.Management.Domains
{
    public class Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CompanyProgram Programs { get; set; }
    }
}
