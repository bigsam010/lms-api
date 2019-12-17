using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Wishlist
    {
        public int Wishid { get; set; }
        public string Coursecode { get; set; }
        public string Customer { get; set; }
        public DateTime? Dateadded { get; set; }=DateTime.Now;
    }
}
