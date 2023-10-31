using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Adherence : DefaultType
    {
        public Adherence(Guid Code, string Description, string Value) : base(Code, Description, Value)
        {
        }

    }
}