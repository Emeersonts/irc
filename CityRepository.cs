using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using  BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class CityRepository : ICityRepository
    {
        private readonly IDataContext dataContext;

        public CityRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<CityView[]> GetCitiesByState(string stateCode)
        {
            var cities = new List<CityView>();

            using(var connectionContext = dataContext.AcquireConnection())
            {
                var sql = @"SELECT 
                              [State] as [StateId]
                              ,[City_Code] as [Id]
                              ,RTRIM([City]) as [Name]
                            FROM [dbo].[Str_ZIP_City]
                            WHERE [State] = @StateId order by Name
                            ";
                cities = (await connectionContext.QueryAsync<CityView>(sql, new { StateId = stateCode})).ToList();
                var index = cities.FindIndex(x => x.Id == 0);
                var item = cities[index];
                cities.RemoveAt(index);
                cities.Insert(0,item);
            }

            return cities.ToArray();
        }

        public async  Task<int> GetTotalCityCountByState(string stateCode)
        {
            var totalCities = 0;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sql = @"IF @StateId <> 'ZZBR'
                            BEGIN
                            SELECT 
                              COUNT(*) -1
                            FROM [dbo].[Str_ZIP_City]
                            WHERE [State] = @StateId 
                            END
                            ELSE
                            BEGIN 
                              SELECT 
                              COUNT(*)
                              FROM [dbo].[Str_ZIP_City]
                              WHERE City <> 'Todas' 
                            END";
                totalCities = (await connectionContext.QueryAsync<int>(sql, new { StateId = stateCode })).ToList().First();
               
            }

            return totalCities;
        }


        /*IF @StateId <> 'ZZBR'
                            BEGIN
                            SELECT 
                              [State] as [StateId]
                              ,[City_Code] as [Id]
                              ,RTRIM([City]) as [Name]
                            FROM [dbo].[Str_ZIP_City]
                            WHERE [State] = @StateId order by Name
                            END
                            ELSE
                            BEGIN 
                              SELECT 
                              [State] as [StateId]
                              ,[City_Code] as [Id]
                              ,RTRIM([City]) as [Name]
                              FROM [dbo].[Str_ZIP_City]
                            END*/
    }
}
