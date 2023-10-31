using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface IMembershipService
    {
        Task<Membership[]> FindAllMembership();
    }
}
