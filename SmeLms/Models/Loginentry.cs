using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Loginentry {
        public int Id { get; set; }
        public string Client { get; set; }
        public DateTime Logindate { get; set; } = DateTime.Now;
    }
}