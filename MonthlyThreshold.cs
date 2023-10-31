namespace BackOffice.Authorizer.Management.Domains
{
    public class MonthlyThreshold
    {
        public Campaign Campaign { get; set; }
        public int InitialQuantity { get; set; }
        public int EndQuantity { get; set; }
        public char RestartQuantity { get; set; }
        public short MaxPeriod { get; set; }
        public decimal Discount { get; set; }
        public short Mileage { get; set; }
        public short RemoveQuantity { get; set; }
        public short RestartCicle { get; set; }
        public string DiscountInfo { get; set; }
        public char Status { get; set; }
        public User Creator { get; set; }
    }
}
