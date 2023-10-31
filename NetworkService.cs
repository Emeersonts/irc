using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Core.Contracts;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Persistence.Api;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class NetworkService : INetworkService
    {
        private readonly INetworkRepository repository;

        public NetworkService(INetworkRepository repository)
        {
            this.repository = repository;
        }

        public async Task<NetworkView> GetNetworkByFiscalNumberRoot(NetworkSearchParameters parameters)
        {
            return await this.repository.GetNetworkByFiscalNumberRoot(parameters);
        }

        public async Task<NetworkView[]> GetNetworksByName(NetworkSearchParameters parameters)
        {
            return await this.repository.GetNetworksByName(parameters);
        }
    }
}
