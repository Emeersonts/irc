using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface ICompanyProgramRepository
    {
        Task<ProgramView[]> GetProgramsByCompany(int acquirerCode);

        Task<ProgramView[]> GetProgramsById(CompanyProgramSearchParameters parameters);
    }
}
