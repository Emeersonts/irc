using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class DefaultType
    {
        public Guid Code { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }

        public DefaultType(Guid code, string description, string value)
        {
            this.Code = code;
            this.Name = description;
            this.Value = value;
        }

    }
}
