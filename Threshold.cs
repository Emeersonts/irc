using BackOffice.Authorizer.Management.Domains.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Threshold
    {
        public DateTime BeginDate { get; set; }
        public Campaign Campaign { get; set; }
        public short InitialQuantity { get; set; }
        public short FinalQuantity { get; set; }
        public char RestartQuantity { get; set; }
        public short RemoveQuantity { get; set; }
        public short PeriodMax { get; set; }
        public decimal Discount { get; set; }
        public decimal Mileage { get; set; }
        public char RecordStatus { get; set; }
        public string RecordStatusInfo { get; set; }
        public string RecordInsertionUser { get; set; }
        public string RecordUpdateUser { get; set; }
    }
}
