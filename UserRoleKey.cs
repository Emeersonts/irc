using IDP.Common.Globalization;
using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class UserRoleKey
    {
        public static UserRoleKey From(Guid value)
        {
            return new UserRoleKey(value);
        }

        public static UserRoleKey FromString(string value)
        {
            if (!Guid.TryParse(value, out var guid))
            {
                throw new ArgumentException(Message.ForKey("Invalid.Guid.Conversion", new string[] { value }).Label);
            }
            return new UserRoleKey(guid);
        }

        public Guid Value { get; private set; }
        private UserRoleKey(Guid value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is UserRoleKey key &&
                   Value.Equals(key.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }


    }
}