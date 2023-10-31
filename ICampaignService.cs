using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface ICampaignService
    {
        Task<CampaignView[]> GetCampaignsByAcquirerCode(CampaignSearchParameters parameters);
        Task<CampaignViewStatus[]> GetActiveCampaigns(string acquirerCode);
        Task<CampaignProdutcView[]> GetCampaignsByProduct(CampaignSearchByProductParameters parameters);
        Task<int> RegisterCampaign(Campaign campaign);
        Task<Campaign> GetCampaignById(CampaignSearchParameters parameters);
        Task<bool> DeactivateCampaign(CampaignDeactivationParameters parameters);
        Task<bool> ActivateCampaign(CampaignDeactivationParameters parameters);
        Task<int> EditCampaign(Campaign campaign);
        Task<Campaign> GetCampaignBasicInformationById(ActiveAndExpiredCampaignSearchParameters parameters);
        Task<bool> DraftCampaignExists(int id, int acquirerCode);
    }
}
