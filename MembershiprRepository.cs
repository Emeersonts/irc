using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class MembershiprRepository : IMembershipRepository
    {

        private readonly IDataContext dataContext;

        public MembershiprRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<Membership[]> FindAllMembership()
        {
            var modesList = new List<Membership>();

            var query = @"SELECT Code,Name, Value FROM [Membership] ORDER BY NAME";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var menbershiplistentity = await connectionContext.QueryAsync<MembershipEntity>(query);

                foreach (var menbership in menbershiplistentity)
                {
                    modesList.Add(new Membership(menbership.Code, menbership.Name, menbership.Value));
                }

                return modesList.ToArray();
            }

        }

    }
}
