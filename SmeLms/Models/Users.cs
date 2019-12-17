using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Users {

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? Dateadded { get; set; } = DateTime.Now;
        public string Password { get; set; }
        public DateTime Lastlogin { set; get; } = DateTime.Now;

        public byte Isprivileged { get; set; } = 0;
    }
}