using BackOffice.Authorizer.Management.Domains;
using SCS.Client.Library.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IAuthenticationRepository
    {
        Task<RegisterUserDTO> CreateUserCredentials(UserCredentials beneficiaryCredential);
    }
}
