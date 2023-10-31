using System;
using System.Collections.Generic;
using System.Linq;

namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignConfigurationContext : ICampaignConfiguration
    {
        private readonly HashSet<CampaignConfiguration> configurations;

        public CampaignConfiguration[] Configurations { get { return configurations.ToArray(); } }

        public CampaignConfigurationContext()
        {
            configurations = new HashSet<CampaignConfiguration>();
        }

        public void AddConfiguration(CampaignConfiguration configuration)
        {
            configurations.Add(configuration);
        }
    }
}