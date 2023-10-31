using IDP.Common.Exceptions;
using IDP.Common.Globalization;
using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Domains.DTOs.Views;
using BackOffice.Authorizer.Management.Persistence.Api;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace BackOffice.Authorizer.Management.Core
{
    public class CampaignService : ICampaignService
    {

        private readonly ICampaignRepository repository;

        public CampaignService (ICampaignRepository campaingnRepository)
        {
            this.repository = campaingnRepository;
        }

        public async Task<bool> ActivateCampaign(CampaignDeactivationParameters parameters)
        {
            return await repository.ActivateCampaign(parameters);
        }

        public async Task<bool> DeactivateCampaign(CampaignDeactivationParameters parameters)
        {
            return await repository.DeactivateCampaign(parameters);
        }

        public async Task<bool> DraftCampaignExists(int id, int acquirerCode)
        {
            return await repository.DraftCampaignExists(id, acquirerCode);
        }

        public async Task<int> EditCampaign(Campaign campaign)
        {
            return await repository.EditCampaign(campaign);
        }

        public async Task<CampaignViewStatus[]> GetActiveCampaigns(string acquirerCode)
        {
            return await repository.GetActiveCampaigns(acquirerCode);
        }

        public async Task<Campaign> GetCampaignBasicInformationById(ActiveAndExpiredCampaignSearchParameters parameters)
        {
            return await repository.GetCampaignBasicInformationById(parameters);
        }

        public async Task<Campaign> GetCampaignById(CampaignSearchParameters parameters)
        {
            return await repository.GetCampaignById(parameters);
        }

        public async Task<CampaignView[]> GetCampaignsByAcquirerCode(CampaignSearchParameters parameters)
        {
            return await repository.GetCampaignsByAcquirerCode(parameters);
        }

        public async Task<CampaignProdutcView[]> GetCampaignsByProduct(CampaignSearchByProductParameters parameters)
        {
            return await repository.GetCampaignsByProduct(parameters);
        }

        public async Task<int> RegisterCampaign(Campaign campaign)
        {
            var result = await repository.RegisterCampaign(campaign);
             return result;   
        }

    }
}
