using IDP.DBX;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using  BackOffice.Authorizer.Management.Persistence.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class StateRepository : IStateRepository
    {
        private readonly IDataContext dataContext;

        public StateRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<StateViewDTO[]> GetAllStates()
        {
            var states = new List<StateViewDTO>();
            using(var connectionContext = dataContext.AcquireConnection())
            {
                var sql = @"SELECT 
	                        [State] as [Abbreviation],
                            CASE RTRIM([State_Name])
							WHEN 'Geral' THEN 'Todos'
							ELSE RTRIM([State_Name])
							END as [Name]
                          FROM [dbo].[Str_ZIP_State]
                          ORDER BY State_Name";
                states = (await connectionContext.QueryAsync<StateViewDTO>(sql)).ToList();
            }
            var firstState = states.Find(state => state.Abbreviation == "ZZBR");
            states.Remove(firstState);
            states.Insert(0, firstState);
            return states.ToArray();
        }
    }
}
