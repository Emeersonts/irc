using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface ICampaignRepository
    {
        Task<CampaignView[]> GetCampaignsByAcquirerCode(CampaignSearchParameters parameters);
        Task<int> RegisterCampaign(Campaign campaign, CampaignEnvironmentType environmentType = CampaignEnvironmentType.Homolog);
        Task<CampaignViewStatus[]> GetActiveCampaigns(string acquirerCode);
        Task<CampaignProdutcView[]> GetCampaignsByProduct(CampaignSearchByProductParameters parameters);
        Task<Campaign> GetCampaignById(CampaignSearchParameters parameters);
        Task<bool> DeactivateCampaign(CampaignDeactivationParameters parameters);
        Task<bool> ActivateCampaign(CampaignDeactivationParameters parameters);
        Task<int> EditCampaign(Campaign campaign);
        Task<Campaign> GetCampaignBasicInformationById(ActiveAndExpiredCampaignSearchParameters parameters);
        Task<bool> DraftCampaignExists(int id, int acquirerCode);
    }
}
