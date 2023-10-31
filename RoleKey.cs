using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class RoleKey
    {
        public static RoleKey From(Guid value)
        {
            return new RoleKey(value);
        }
        public Guid Value { get; private set; }
        private RoleKey(Guid value)
        {
            Value = value;
        }
    }
}