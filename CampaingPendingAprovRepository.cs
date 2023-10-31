using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class CampaingPendingAprovRepository :ICampaingPendingAprovRepository
    {
        private readonly IDataContext dataContext;

        public CampaingPendingAprovRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<CampaingPendingAprov[]> GetCampaingPendingAprov(CampaingPendingAprovParameters parameter)
        {
            string sql = @"SELECT
            [Acquirer_Code], [Rules_Code],[rules_name],[STATUSNAME] = CASE Rec_St WHEN 'A' THEN 'Ativo'
            WHEN 'D' THEN 'D' WHEN 'P' THEN 'Pendente' ELSE 'Sem definição' END, Rec_St FROM [dbo].[Rules_MsFile]
            WHERE[TktMsg_RulesInfo] = @Laboratory";
            
            var ListCampPend = new List<CampaingPendingAprov>();
            using (var connectionContext = dataContext.AcquireConnection())
            {
                ListCampPend = (await connectionContext.QueryAsync<CampaingPendingAprov>(sql, new { Laboratory = parameter.Laboratory })).ToList();
            }
            return ListCampPend.ToArray();

        }
    }
}
