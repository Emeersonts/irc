using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Core.Contracts;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository repository;

        public QuestionnaireService(IQuestionnaireRepository repository)
        {
            this.repository = repository;
        }

        public async Task<bool> CheckQuestionnaireCanBeSaved(int acquirerCode, int brandId)
        {
            return await repository.CheckQuestionnaireCanBeSaved(acquirerCode, brandId);
        }
    }
}
