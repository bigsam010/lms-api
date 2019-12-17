using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Planchangerequest {
        public int Id { get; set; }
        public string Customer { get; set; }
        public string Type { get; set; }
        public string Paymentref { get; set; }
        public DateTime Datechanged { get; set; } = DateTime.Now;
        public int Oldplan { get; set; }
        public int Newplan { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }

    }
}