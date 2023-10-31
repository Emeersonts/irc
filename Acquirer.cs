namespace BackOffice.Authorizer.Management.Domains
{
    public class Acquirer
    {
        public int AcquirerCode { get; private set; }
        public string AcquirerName { get; set; }
        public bool IsActive { get; set; }

        public Acquirer(int acquirerCode)
        {
            AcquirerCode = acquirerCode;
        }
    }
}
