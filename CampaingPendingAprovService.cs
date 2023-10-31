using BackOffice.Authorizer.Management.Core.Contracts;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class CampaingPendingAprovService : ICampaingPendingAprovService
    {
        private readonly ICampaingPendingAprovRepository repository;

        public CampaingPendingAprovService(ICampaingPendingAprovRepository repository)
        {
            this.repository = repository;
        }
        public async Task<CampaingPendingAprov[]> GetCampaingPendingAprovService(CampaingPendingAprovParameters parameter)
        {
            try
            {
                return await this.repository.GetCampaingPendingAprov(parameter);
            }
            catch
            {
                throw new NotImplementedException("Erro imprevisto na implementação da API");
            }
        }

    }
}
