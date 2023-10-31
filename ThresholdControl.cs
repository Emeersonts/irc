using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class ThresholdControl : DefaultType
    {
        public ThresholdControl(Guid Code, string Description, string Value) : base(Code, Description, Value)
        {
        }

    }
}
