using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Domains
{
    public class StateComparer : IEqualityComparer<State>
    {
        public bool Equals(State x, State y)
        {
           return x.Abbreviation == y.Abbreviation;
        }

        public int GetHashCode(State obj)
        {
            return obj.Abbreviation.GetHashCode();
        }
    }
}
