using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Property
    {
        public PropertyKey Key { get; private set; }
        public string Description { get; private set; }

        public Property(PropertyKey key, string description)
        {
            this.Key = key;
            this.Description = description;
        }

        public override bool Equals(object obj)
        {
            return obj is Property property &&
                   EqualityComparer<PropertyKey>.Default.Equals(Key, property.Key) &&
                   Description == property.Description;
        }

        public override int GetHashCode()
        {
            int hashCode = -1578784523;
            hashCode = hashCode * -1521134295 + EqualityComparer<PropertyKey>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            return hashCode;
        }
    }
}
