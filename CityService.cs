using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class CityService : ICityService
    {
        private readonly ICityRepository repository;

        public CityService(ICityRepository repository)
        {
            this.repository = repository;
        }
        public async Task<CityView[]> GetCitiesByState(string stateCode)
        {
            return await repository.GetCitiesByState(stateCode);
        }
    }
}
