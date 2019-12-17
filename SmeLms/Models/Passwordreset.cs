using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Passwordreset
    {
        public int Id { get; set; }
        public string Customer { get; set; }
        public string Token { get; set; }
        public DateTime? Expdate { get; set; }
    }
}
