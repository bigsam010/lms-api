using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Inclassregistration {
        public string Regid { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public int? Classid { get; set; }
        public DateTime? Regdate { get; set; } = DateTime.Now;
        public string Invitedby { get; set; }
        public string Type { get; set; }
    }
}