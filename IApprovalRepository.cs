using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IApprovalRepository
    {
        Task<Approval> FindApprovalById(int id);
        Task<bool> UpdateApproval(Approval approval);
        Task<int> CreateApproval(Approval approval);
        Task<StatusKey[]> GetAllStatus();
        Task<PageableResult> FindApprovalBy(ApprovalListParameter parameters);
    }
}
