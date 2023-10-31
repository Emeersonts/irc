using IDP.Common.Math;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Coupon
    {
        public RangeKey RangeKey { get; set; }

        public Fraction MinValue { get; private set; }

        public Fraction MaxValue { get; private set; }

        public string Description { get; set; }

        public Coupon(RangeKey rangeKey, Fraction minValue, Fraction maxValue)
        {
            this.RangeKey = rangeKey;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }
    }
}
