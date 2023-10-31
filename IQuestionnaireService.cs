using BackOffice.Authorizer.Management.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface IQuestionnaireService
    {
        Task<bool> CheckQuestionnaireCanBeSaved(int acquirerCode, int brandId);
    }
}
