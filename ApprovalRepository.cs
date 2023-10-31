using IDP.DBX;
using BackOffice.Authorizer.Management.Domains;
using  BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly IDataContext dataContext;

        public ApprovalRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<Approval> FindApprovalById(int id)
        {
            Approval ret;
            const string sql =
                @"SELECT TOP 1 *
					from [dbo].[ApprovalRequest]
                where Id = @Id";

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var approval = await connectionContext.QueryAsync<ApprovalEntity>(
                         sql, new { Id = id });

                ret = approval.Select(x => new Approval(x.Id, x.Acquirer_Code, x.RequestType, x. RequestId, x.Status, x.RequestInfo, x.RecInsTime, x.RecInsUser, x.RecUpdTime, x.RecUpdUser)).FirstOrDefault();
            }

            return ret;
        }

        public async Task<PageableResult> FindApprovalBy(ApprovalListParameter parameters)
        {
            var listApproval = new List<ApprovalViewDTO>();
            var condition = "AND (@Name IS NULL)";
            if (!string.IsNullOrWhiteSpace(parameters.FilterText))
            {
                condition = "AND (RF.Rules_Name LIKE '%' + @Name + '%')";
            }
            string sql =
                @"SELECT* FROM
                (
                    SELECT ROW_NUMBER() OVER(ORDER BY sub.[Statusbit] desc) as linha, *FROM
                    (
						 SELECT
                        AR.[Id]
                        , RF.Rules_Code AS campaignCode
                        , RF.Rules_Name AS campaignName
                        ,CASE [STATUS]
						 WHEN 'R' THEN 'RECUSADA'
						 WHEN 'P' THEN 'PENDENTE APROVAÇÃO'
						END AS [STATUS]
                        , IIF(AR.[STATUS] = 'P', CAST(1 AS BIT), CAST(0 AS BIT)) AS[Statusbit]
                        FROM[dbo].[ApprovalRequest]
                        AR
                        JOIN [dbo].rules_msfile rf
                        ON AR.RequestId = rf.Rules_Code
                        AND AR.Acquirer_Code = RF.Acquirer_Code
                        WHERE AR.RequestType = @RequestType
                         AND AR.Acquirer_Code = @AcquirerCode
                            AND (AR.[Status] = @Status)
                        "+ condition + ") as sub) as tre where tre.linha >= @Start and tre.linha < @End";

                string sqlCount = @"SELECT   COUNT( AR.[Id]) AS total    FROM[dbo].[ApprovalRequest] AR   JOIN [dbo].rules_msfile rf  ON AR.RequestId = rf.Rules_Code
                AND AR.Acquirer_Code = RF.Acquirer_Code
                WHERE AR.RequestType = @RequestType
                    AND AR.Acquirer_Code = @AcquirerCode
                    AND (AR.[Status] = @Status) " + condition;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var statusMapping = new Dictionary<string, string>();
                statusMapping.Add("PENDENTE APROVAÇÃO", "P");
                statusMapping.Add("RECUSADA", "R");

                var countItems = (await connectionContext.QueryAsync<long>(sqlCount, new { acquirercode = parameters.AcquirerCode, RequestType = "CAM", 
                    Status = !string.IsNullOrWhiteSpace(parameters.Status) && statusMapping.ContainsKey(parameters.Status)? statusMapping[parameters.Status]: null , Name = parameters.FilterText })).First();

                var approvals = await connectionContext.QueryAsync<ApprovalViewDTO>(
                         sql, new { acquirerCode = parameters.AcquirerCode, RequestType = "CAM", Start = parameters.StartIndex, End = parameters.End,
                         Status = !string.IsNullOrWhiteSpace(parameters.Status) && statusMapping.ContainsKey(parameters.Status) ? statusMapping[parameters.Status] : null, Name = parameters.FilterText});

                listApproval.AddRange(approvals);

                return PageableResult.From(parameters.StartIndex, parameters.TotalPerPages, countItems, listApproval.ToArray());

            }
            
        }
        
        public async Task<bool> UpdateApproval(Approval approval)
        {
            bool recordsAffected = false;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                recordsAffected = (await connectionContext.UpdateAsync<ApprovalEntity>(new ApprovalEntity()
                {
                    Id = approval.Id,
                    Acquirer_Code = approval.AcquirerCode,
                    RequestType = approval.RequestType,
                    RequestId = approval.RequestId,
                    RequestInfo = approval.RequestInfo,
                    Status = approval.Status,
                    RecInsTime = approval.RecInsTime,
                    RecInsUser = approval.RecInsUser,
                    RecUpdTime = approval.RecUpdTime,
                    RecUpdUser = approval.RecUpdUser
                }));
            }

            return recordsAffected;
        }

        public async Task<int> CreateApproval(Approval approval)
        {
            int recordsAffected = 0;

            using (var connectionContext = dataContext.AcquireConnection())
            {
                recordsAffected = (await connectionContext.InsertAsync<ApprovalEntity>(new ApprovalEntity()
                {
                    Acquirer_Code = approval.AcquirerCode,
                    RequestType = approval.RequestType,
                    RequestId = approval.RequestId,
                    RequestInfo = approval.RequestInfo,
                    Status = approval.Status,
                    RecInsTime = approval.RecInsTime,
                    RecInsUser = approval.RecInsUser,
                    RecUpdTime = approval.RecUpdTime,
                    RecUpdUser = approval.RecUpdUser
                }));
            }

            return recordsAffected;
        }

        public async Task<StatusKey[]> GetAllStatus()
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var sql = @"SELECT [Description] FROM ApproverStatus";

                var statusList = (await connectionContext.QueryAsync<dynamic>(sql))?.Select(status =>
                new StatusKey(status.Description.ToString())).ToArray();

                return statusList;
            }
        }
    }
}
