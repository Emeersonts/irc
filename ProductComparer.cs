using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class ProductComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product x, Product y)
        {
            return x.EAN == y.EAN;
        }

        public int GetHashCode(Product obj)
        {
            return obj.EAN.GetHashCode();
        }
    }
}
