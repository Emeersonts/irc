using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class RangeKey
    {
        public static RangeKey From(long value)
        {
            return new RangeKey(value);
        }
        public long Value { get; private set; }
        private RangeKey(long value)
        {
            Value = value;
        }
    }
}
