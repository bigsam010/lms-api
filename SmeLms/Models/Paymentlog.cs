using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Paymentlog
    {
        public string Refno { get; set; }
        public string Customer { get; set; }
        public string Description { get; set; }
        public string Paymentmode { get; set; }
        public DateTime? Paymentdate { get; set; } = DateTime.Now;
        public string Cardnumber { get; set; }
        public string Status { get; set; }
        public decimal Cashamount { get; set; } = 0;
        public int Loyalitypoints { get; set; } = 0;
        public string Itemref { get; set; }
        public string Itemdescription { set; get; }
    }
}