using BackOffice.Authorizer.Management.Domains.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignAdherenceModel
    {
        public Campaign Campaign { get; set; }
        public string AdherenceDataType { get; set; }
    }
}
