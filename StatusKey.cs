using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class StatusKey
    {
        public StatusKey(string value)
        {
            Value = value;
        }
        public string Value { get; private set; }
       
    }
}
