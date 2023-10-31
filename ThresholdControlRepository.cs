using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class ThresholdControlRepository : IThresholdControlRepository
    {
        
        private readonly IDataContext dataContext;
        public ThresholdControlRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        
        public async Task<ThresholdControl[]> FindAllThresholdControl()
        {
            var thresholdList = new List<ThresholdControl>();

            var query = @"SELECT Code,Name, Value FROM [ThresholdControl] ORDER BY NAME";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var Thresholdlistentity = await connectionContext.QueryAsync<ThresholdControlEntity>(query);

                foreach (var thresholdControl in Thresholdlistentity)
                {
                    thresholdList.Add(new ThresholdControl(thresholdControl.Code, thresholdControl.Name, thresholdControl.Value));
                }

                return thresholdList.ToArray();
            }
        }
        
    }
}