using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class ComboCampaign
    {
        public Company Company { get; set; }
        public DateTime BeginDate { get; set; }
        public Campaign Campaign1 { get; set; }
        public Campaign Campaign2 { get; set; }
        public Product Product1 { get; set; }
        public Product Product2 { get; set; }
        public char Status { get; set; }
        public short Quantity1 { get; set; }
        public short Quantity2 { get; set; }
        public string UserName { get; set; }
    }   
}
