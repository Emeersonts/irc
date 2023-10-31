using BackOffice.Authorizer.Management.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IQuestionnaireRepository
    {

        Task<int> SaveQuestionnaire(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);

        Task<bool> CheckQuestionnaireCanBeSaved(int acquirerCode, int brandId);

    }
}
