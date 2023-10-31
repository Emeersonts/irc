using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaingPendingAprov
    {
        public int Acquirer_Code { get; set; }
        public int Rules_Code { get; set; }        
        public string rules_name { get; set; }
        public string STATUSNAME { get; set; }
        public string Rec_St { get; set; }
    }
}
