using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Beneficiary {
        public string Email { get; set; }
        public string Addedby { get; set; }
        public DateTime? Dateadded { get; set; } = DateTime.Now;
        public string Status { get; set; }
        public byte Ispriviledge { get; set; }
       
    }
}