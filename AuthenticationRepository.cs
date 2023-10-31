using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using SCS.Client.Library;
using SCS.Client.Library.ViewItems;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IScsClient scsClient;

        public AuthenticationRepository (IScsClient scsClient)
        {
            this.scsClient = scsClient;
        }

        public async Task<RegisterUserDTO> CreateUserCredentials (UserCredentials beneficiaryCredential)
        {  
            return await scsClient.RegisterActiveUser(beneficiaryCredential.Name, beneficiaryCredential.Email, beneficiaryCredential.Password, beneficiaryCredential.SocialNumber);       
        }
    }
}
