using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Utilities
{
    public static class ValidationUtilities
    {
        public static bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out var newGuid);
        }
    }
}
