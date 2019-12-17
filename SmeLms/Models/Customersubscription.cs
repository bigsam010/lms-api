using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Customersubscription {
        public int Id { get; set; }
        public string Customer { get; set; }
        public string Paymentref { get; set; }
        public DateTime? Expdate { get; set; }
        public byte Autorenew { get; set; } = 1;
        public DateTime? Subdate { get; set; } = DateTime.Now;
        public int Subid { get; set; }
        public string Status { get; set; } = "Active";
    }
}