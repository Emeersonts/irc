using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class ApprovalListParameter : SearchParameters
    {
        public static ApprovalListParameter Create(int startIndex, int totalPerPages, int acquirerCode, string status, string filterText)
        {
            return new ApprovalListParameter(startIndex, totalPerPages, acquirerCode, status, filterText);
        }
        
        public int AcquirerCode { get; private set; }
        public int IdUserLog { get; set; }
        public string Status { get; set; }

        private ApprovalListParameter(int startIndex, int totalPerPages, int acquirerCode, string status, string filterText)
            : base(startIndex, totalPerPages)
        {
            AcquirerCode = acquirerCode;
            Status = status;
            FilterText = !string.IsNullOrWhiteSpace(filterText)?filterText:null;
        }
    }
}
