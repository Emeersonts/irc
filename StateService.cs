using BackOffice.Authorizer.Management.Core.Contracts;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using BackOffice.Authorizer.Management.Persistence;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class StateService : IStateService
    {
        private readonly IStateRepository repository;

        public StateService(IStateRepository repository)
        {
            this.repository = repository;
        }

        public async Task<StateViewDTO[]> GetAllStates()
        {
            return await repository.GetAllStates();
        }
    }
}
