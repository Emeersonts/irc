using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Product
    {
        public bool AllowCRM { get; set; }
        public string EAN { get; set; }
        public State[] States { get; set; }
        public DateTime BeginDate { get; set; }
        public Brand Brand { get; set; }
        public Campaign Campaign { get; set; }
        public string Description { get; set; }
        public char MultiProductId { get; set; }
        public short InitialQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public short FinalQuantity { get; set; }
        public short PeriodMax { get; set; }
        public bool AssociatedWithCombo { get; set; }
        public decimal Discount { get; set; }
        public decimal Mileage { get; set; }
        public char DiscountType { get; set; }
        public int CurrentBillQuantity { get; set; }
        public decimal CurrentBillDiscount { get; set; }
        public short TrnProfessionType { get; set; }
        public decimal PercentageProductParticipationByAgreement { get; set; }
        public decimal PercentageProductParticipationByIssuer { get; set; }
        public string AttendInfo { get; set; }
        public char RecordStatus { get; set; }
        public string RecordStatusInfo { get; set; }
        public char RulesComboId { get; set; }
        public string RecordInsertionUser { get; set; }
        public string RecordUpdateUser { get; set; }

        public Product()
        {
            this.Campaign = new Campaign();
        }
    }
}
