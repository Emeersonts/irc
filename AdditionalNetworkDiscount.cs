using Quartz.Impl.Triggers;
using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class AdditionalNetworkDiscount
    {
        public Product Product { get; private set; }
        public string FiscalNumberRoot { get; private set; }
        public State State { get; private set; }
        public Decimal Discount { get; private set; }
        public Campaign Campaign { get; set; }

        public string NetworkName { get; set; }

        public AdditionalNetworkDiscount(Product product, string fiscalNumberRoot, State state, decimal discount, string networkName = "")
        {
            Product = product;
            FiscalNumberRoot = fiscalNumberRoot;
            State = state;
            Discount = discount;
            NetworkName = networkName;
        }
    }
}
