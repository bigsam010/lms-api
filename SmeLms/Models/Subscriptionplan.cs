using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Subscriptionplan {
        public int Subid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Cycle { get; set; }
        public string Status { get; set; } = "Active";
        public string Type { get; set; }
        public int Beneficiarycount { get; set; } = 0;
        public decimal Amount { get; set; }
        public int Classcount { get; set; }
        public int Coursecount { get; set; }
    }
}