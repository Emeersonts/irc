using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public enum CampaignStatus
    {
        APROVADA,
        ATIVA,
        DESATIVADA,
        [Description("EM APROVAÇÃO")]
        EM_APROVACAO,
        EXPIRADA,
        RECUSADA
    }
}
