using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class PropertyKey
    {
        public static PropertyKey From(Guid value)
        {
            return new PropertyKey(value);
        }

        public static PropertyKey New()
        {
            return new PropertyKey(Guid.NewGuid());
        }

        public Guid Value { get; private set; }

        private PropertyKey(Guid value)
        {
            Value = value;
        }
    }
}
