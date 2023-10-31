using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IModesRepository
    {
        Task<Modes[]> FindAllModess();
    }
}