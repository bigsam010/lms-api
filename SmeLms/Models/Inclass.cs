using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Inclass
    {
        public int Classid { get; set; }
        public string Title { get; set; }
        public decimal? Duration { get; set; }
        public DateTime Startdate { get; set; } = DateTime.Now.Date;
        public string Coursedescription { get; set; }
        public string Timedescription { get; set; }
        public string Objectives { get; set; }
        public int Loyalitypoint { get; set; } = 0;
        public string Starttime { get; set; }
        public string Catid { get; set; }
        public DateTime Enddate { get; set; } = DateTime.Now.Date.AddDays(1);
        public string Status { get; set; } = "Upcoming";
        public string Createdby { get; set; }
        public decimal Price { set; get; }
        public string Location { set; get; }
    }
}