using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Core
{
    public class ApprovalService : IApprovalService
    {

        private readonly IApprovalRepository repository;
        private readonly ICampaignRepository campaignRepository;
        public ApprovalService (IApprovalRepository approvalRepository, ICampaignRepository campaignRepository)
        {
            this.repository = approvalRepository;
            this.campaignRepository = campaignRepository;
        }

        public async Task<PageableResult> FindApprovalBy(ApprovalListParameter parameters)
        {
            return await repository.FindApprovalBy(parameters);
        }

        public async Task ApproveCampaign(CampaignApproveDTO dto)
        {
            var approval = await this.repository.FindApprovalById(dto.Id);
            approval.Approve(dto.UserName);

            await this.repository.UpdateApproval(approval);

            var campaign = await campaignRepository.GetCampaignById(new CampaignSearchParameters(approval.AcquirerCode.ToString(), approval.RequestId));

            if(campaign != null)
            {
                await this.campaignRepository.RegisterCampaign(campaign, CampaignEnvironmentType.Production);
            }
        }

        public async Task DeclineCampaign(CampaignDeclineDTO dto)
        {
            var approval = await this.repository.FindApprovalById(dto.Id);

            approval.Decline(dto.UserName, dto.Description);

            await this.repository.UpdateApproval(approval);
        }

        public async Task<int> SubmitForApproval(CampaignSubmitApprovalDTO dto)
        {
            Approval approval = new Approval(0, dto.AcquirerCode, "CAM", dto.CampaignId, "P", "", DateTime.Now, dto.UserName, DateTime.Now, dto.UserName);
            return await this.repository.CreateApproval(approval);
        }

        public async Task<StatusKey[]> GetAllStatus()
        {
            return await repository.GetAllStatus();
        }
    }
}
