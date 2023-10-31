using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Discount : DefaultType
    {
        public Discount(Guid Code, string Description, string Value) : base(Code, Description, Value)
        {
        }

    }
}
