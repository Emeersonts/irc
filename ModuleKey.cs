using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class ModuleKey
    {
        public static ModuleKey From(Guid value)
        {
            return new ModuleKey(value);
        }

        public Guid Value { get; private set; }

        private ModuleKey(Guid value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleKey key &&
                   Value.Equals(key.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }   


    }
}
