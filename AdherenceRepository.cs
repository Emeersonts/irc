using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class AdherenceRepository : IAdherenceRepository
    {

        private readonly IDataContext dataContext;

        public AdherenceRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<Adherence[]> FindAllAdherence()
        {
            var adherenceList = new List<Adherence>();

            var query = @"SELECT Code,Name, Value FROM [Adherence] ORDER BY NAME";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var adherencelistentity = await connectionContext.QueryAsync<AdherenceEntity>(query);

                foreach (var adherence in adherencelistentity)
                {
                    adherenceList.Add(new Adherence(adherence.Code, adherence.Name, adherence.Value));
                }

                return adherenceList.ToArray();
            }
        }

    }
}
