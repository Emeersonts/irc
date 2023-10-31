using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface INetworkService
    {
        Task<NetworkView> GetNetworkByFiscalNumberRoot(NetworkSearchParameters parameters);

        Task<NetworkView[]> GetNetworksByName(NetworkSearchParameters parameters);
    }
}
