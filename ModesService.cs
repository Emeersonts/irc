using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class ModesService : IModesService
    {

        private readonly IModesRepository modesrepository;

        public ModesService(IModesRepository modesrepository)
        {
            this.modesrepository = modesrepository;
        }

        public async Task<Modes[]> FindAllModes()
        {
            return await modesrepository.FindAllModess();
        }
    }
}
