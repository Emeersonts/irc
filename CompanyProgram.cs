namespace BackOffice.Authorizer.Management.Domains
{
    public class CompanyProgram
    {
		public int Id { get; set; }
        public string Name { get; set; }
        public Company Company { get; set; }
        public Campaign[] Campaigns {get; set;}
	}
}
