using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class ThresholdControlService : IThresholdControlService
    {

        private readonly IThresholdControlRepository thresholdControlrepository;

        public ThresholdControlService(IThresholdControlRepository thresholdControlrepository)
        {
            this.thresholdControlrepository = thresholdControlrepository;
        }

        public async Task<ThresholdControl[]> FindAllThresholdControl()
        {
            return await thresholdControlrepository.FindAllThresholdControl();
        }
    }
}
