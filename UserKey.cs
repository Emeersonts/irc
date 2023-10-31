using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class UserKey
    {
        public static UserKey From(Guid value)
        {
            return new UserKey(value);
        }

        public Guid Value { get; private set; }

        private UserKey(Guid value)
        {
            Value = value;
        }
        
    }
}
