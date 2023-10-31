using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface IApprovalService
    {
        Task<PageableResult> FindApprovalBy(ApprovalListParameter parameters);
        Task ApproveCampaign(CampaignApproveDTO dto);
        Task DeclineCampaign(CampaignDeclineDTO campaign);
        Task<StatusKey[]> GetAllStatus();
        Task<int> SubmitForApproval(CampaignSubmitApprovalDTO campaign);
    }
}
