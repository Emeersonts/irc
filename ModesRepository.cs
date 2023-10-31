using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class ModesRepository : IModesRepository
    {
        
        private readonly IDataContext dataContext;

        public ModesRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }


        public async Task<Modes[]> FindAllModess()
        {
            var modesList = new List<Modes>();

            var query = @"SELECT Code,Name, Value FROM [Modes] ORDER BY NAME";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var modeslistentity = await connectionContext.QueryAsync<ModesEntity>(query);

                foreach (var modes in modeslistentity)
                {
                    modesList.Add(new Modes(modes.Code, modes.Name, modes.Value));
                }

                return modesList.ToArray();
            }
        }

    }
}
