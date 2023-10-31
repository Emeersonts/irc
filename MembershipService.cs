using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class MembershipService : IMembershipService
    {
        private readonly IMembershipRepository MembershipRepository;

        public MembershipService(IMembershipRepository MembershipRepository)
        {
            this.MembershipRepository = MembershipRepository;
        }

        public async Task<Membership[]> FindAllMembership()
        {
            return await MembershipRepository.FindAllMembership();

        }

    }
}
