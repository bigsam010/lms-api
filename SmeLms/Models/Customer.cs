using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Customer {

        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Status { get; set; } = "Active";
        public int Loyalitypoint { get; set; } = 0;
        public string Verificationtoken { get; set; }
        public string Email { get; set; }
        public byte? Isverified { get; set; }
        public DateTime Joindate { get; set; } = DateTime.Now;
        public string Accounttype { get; set; }
        public string Password { get; set; }
        public string Companyname { get; set; }
         public string Accountcategory { get; set; }
    }
}