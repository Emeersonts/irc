using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class AdherenceService : IAdherenceService
    {

        private readonly IAdherenceRepository adherencerepository;

        public AdherenceService(IAdherenceRepository adherencerepository)
        {
            this.adherencerepository = adherencerepository;
        }

        public async Task<Adherence[]> FindAllAdherence()
        {
            return await adherencerepository.FindAllAdherence();
        }

    }
}
