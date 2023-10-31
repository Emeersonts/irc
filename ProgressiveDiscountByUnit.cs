namespace BackOffice.Authorizer.Management.Domains
{
    public class ProgressiveDiscountByUnit
    {
        public Campaign Campaign { get; set; }
        public short InitialQuantity { get; set; }
        public short FinalQuantity { get; set; }
        public decimal ThresholdPunishmentAmount { get; set; }
        public short PunishmentDays { get; set; }
        public decimal Discount { get; set; }
    }
}
