using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 namespace BackOffice.Authorizer.Management.Core.Api
{
    public interface IStateService
    {
        Task<StateViewDTO[]> GetAllStates();
    }
}
