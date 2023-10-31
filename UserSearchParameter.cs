namespace BackOffice.Authorizer.Management.Domains
{
    public class UserSearchParameter : SearchParameters
    {
        public static UserSearchParameter Create(int startIndex, int totalPerPages, int acquirerCode, string filterText)
        {
            return new UserSearchParameter(startIndex, totalPerPages, acquirerCode, filterText);
        }

        public int AcquirerCode { get; private set; }

        private UserSearchParameter(int startIndex, int totalPerPages, int acquirerCode, string filterText)
            : base(startIndex, totalPerPages)
        {
            AcquirerCode = acquirerCode;
            FilterText = filterText;
        }
    }
}
