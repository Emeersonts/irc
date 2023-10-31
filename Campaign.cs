using System;

namespace BackOffice.Authorizer.Management.Domains
{
	public class Campaign
    {
        public int Id { get; set; }
		public string Name { get; set; }
		public int AcquirerCode { get; set; }

		public Coupon Coupon { get; set; }
		public CompanyProgram Program { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime EndDateForPurchase { get; set; }
        public string Status { get; set; }
		public int MaximumNumberOfParticipants { get; set; }
        public int CurrentNumberOfParticipants { get; set; }
		public short BillKitNumberOfDays { get; set; }
		public short ExpKitNumberOfDays { get; set; }
		public char ProductdAdhesionId { get; set; }
		public short LimitMode { get; set; }

		public short StateDiscountType { get; set; }

		public string ThresholdControl { get; set; }

		public MembershipChannel[] Channels { get; set; }

		public short ControlMode { get; set; }
		public string RecStatusInfo { get; set; }
		public string RecordInsertionUser { get; set; }
		public char ProductQuantityLevelId { get; set; }
		public char PriceDesc { get; set; }
		public AssociatedCampaign[] AssociatedCampaigns { get; set; }
		public AdditionalNetworkDiscount[] AdditionalNetworkDiscounts { get; set; }
		public CampaignAdherenceModel AdhrenceModel { get; set; }
		public decimal QuantityPriceDescMax { get; set; }
		public Threshold[] Thresholds { get; set; }
		public Product[] Products { get; set; }
		public ComboBrandCampaign[] ComboBrand { get; set; }
		public ComboCampaign[] Combos { get; set; }
		public CampaignKit[] Kits { get; set; }
		public Network[] Networks { get; set; }
		public CampaignMigration[] MigratedCampaigns { get; set; }
		public MonthlyProgressiveDiscount[] MonthlyDiscounts { get; set; }
		public ProgressiveDiscountByUnit[] UnitaryDiscounts { get; set; }
		public Physician[] Physicians { get; set; }

		public Questionnaire Questionnaire { get; set; }
	}
}
