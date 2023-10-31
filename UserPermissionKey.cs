using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class UserPermissionKey
    {
        public static UserPermissionKey From(Guid value)
        {
            return new UserPermissionKey(value);
        }

        public Guid Value { get; private set; }
        private UserPermissionKey(Guid value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is UserPermissionKey key &&
                   Value.Equals(key.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

       


    }
}
