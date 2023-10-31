using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IThresholdControlRepository
    {
        Task<ThresholdControl[]> FindAllThresholdControl();
    }
}
