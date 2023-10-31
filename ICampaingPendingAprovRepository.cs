using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackOffice.Authorizer.Management.Domains;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface ICampaingPendingAprovRepository
    {
        Task<CampaingPendingAprov[]> GetCampaingPendingAprov(CampaingPendingAprovParameters parameter);

    }
}
