
namespace BackOffice.Authorizer.Management.Domains
{
    public class MonthlyProgressiveDiscount
    {
        public Campaign Campaign { get; set; }
        public short ProductInitialQuantity { get; set; }
        public short ProductFinalQuantity { get; set; }
        public short MaximumPeriodForPunishment { get; set; }
        public decimal ThresholdPunishmentAmount { get; set; }
        public decimal Discount { get; set; }
    }
}
