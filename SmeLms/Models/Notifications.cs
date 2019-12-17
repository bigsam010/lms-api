using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Notifications {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Target { get; set; }
        public string Remark { get; set; }
        public byte Viewed { get; set; } = 0;
        public DateTime Notedate { get; set; } = DateTime.Now;
    }
}