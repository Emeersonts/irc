using BackOffice.Authorizer.Management.Core.Contracts;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackOffice.Authorizer.Management.Core.Api;

namespace BackOffice.Authorizer.Management.Core
{
    public class CompanyProgramService : ICompanyProgramService
    {
        private readonly ICompanyProgramRepository repository;

        public CompanyProgramService( ICompanyProgramRepository repository)
        {
            this.repository = repository;
        }

        public async Task<ProgramView[]> GetProgramsByCompany(int acquirerCode)
        {
            return await this.repository.GetProgramsByCompany(acquirerCode);
        }
    }
}
