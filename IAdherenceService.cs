﻿using BackOffice.Authorizer.Management.Domains;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface IAdherenceService
    {
        Task<Adherence[]> FindAllAdherence();
    }
}
