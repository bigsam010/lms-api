using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Subusage {
        public int Id { get; set; }
        public int Subid { get; set; }
        public int Coursetotal { get; set; }
        public int Classtotal { get; set; }
    }
}