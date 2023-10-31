using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Membership : DefaultType
    {
        public Membership(Guid Code, string Description, string Value) : base(Code, Description, Value)
        {
        }

    }
}
