namespace BackOffice.Authorizer.Management.Domains
{
    public class UserCredentials
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string SocialNumber { get; private set; }
        public UserCredentials(string name, string email, string password, string socialNumber)
        {
            Name = name;
            Email = email;
            Password = password;
            SocialNumber = socialNumber;
        }
    }
}
