using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class OptionKey
    {
        public static OptionKey From(Guid value)
        {
            return new OptionKey(value);
        }

        public static OptionKey New()
        {
            return new OptionKey(Guid.NewGuid());
        }

        public Guid Value { get; private set; }

        private OptionKey(Guid value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is OptionKey key &&
                   Value.Equals(key.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

    }
}
