using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface ICompanyProgramService
    {
        Task<ProgramView[]> GetProgramsByCompany(int acquirerCode);
    }
}
