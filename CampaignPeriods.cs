using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignPeriods : DefaultType
    {
        public CampaignPeriods(Guid Code, string Description, string Value) : base(Code, Description, Value)
        {
        }

    }
}
