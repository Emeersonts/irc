using BackOffice.Authorizer.Management.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IAdherenceRepository
    {
        Task<Adherence[]> FindAllAdherence();
    }
}
