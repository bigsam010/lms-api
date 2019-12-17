using System;
using System.Collections.Generic;

namespace SmeLms {
    public partial class Customercard {
        public string Cardnumber { get; set; }
        public string Customer { get; set; }
        public string Cvv { get; set; }
        public string Expdate { get; set; }
        public DateTime Dateadded { get; set; } = DateTime.Now;
    }
}