namespace BackOffice.Authorizer.Management.Domains
{
    public class UserRoleType
    {
        public int Key { get; private set; }
        public string Type { get; private set; }

        public UserRoleType(int key, string type)
        {
            this.Key = key;
            this.Type = type;
        }
    }
}
