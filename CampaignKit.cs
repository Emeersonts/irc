namespace BackOffice.Authorizer.Management.Domains
{
    public class CampaignKit
    {
        public Campaign Campaign { get; set; }
        public Product Product { get; set; }
        public short Quantity { get; set; }
        public decimal Discount { get; set; }
        public char Status { get; set; }
        public string UserName { get; set; }
    }
}
