using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface ICityRepository
    {
        Task<CityView[]> GetCitiesByState(string stateCode);
        Task<int> GetTotalCityCountByState(string stateCode);
    }
}
