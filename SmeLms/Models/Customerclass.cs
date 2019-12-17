using System;
using System.Collections.Generic;

namespace SmeLms {
    public partial class Customerclass {
        public int Classid { get; set; }
        public string Coursecode { get; set; }
        public DateTime Startdate { get; set; } = DateTime.Now;
        public DateTime? Enddate { get; set; }
        public string Customer { get; set; }
        public string Status { get; set; } = "Ongoing";
        public string Type { get; set; }
    }
}