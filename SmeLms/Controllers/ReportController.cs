using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Net.Mail;



namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : Controller
    {
        smelmsContext _context;
        public ReportController(smelmsContext _context)
        {
            this._context = _context;
        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult<decimal> OverallEarnings()
        {
            return _context.Paymentlog.Where(pl => pl.Status == "Accepted").Sum(pl => pl.Cashamount);
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<decimal> CurrentYearEarnings()
        {
            return _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Year == DateTime.Now.Year && pl.Status == "Accepted").Sum(pl => pl.Cashamount);
        }
        [HttpGet]
        [Route("[action]/{backby}/{revenuetype}")]
        public ActionResult<SortedDictionary<string, decimal>> DailyRevenue(int backby, string revenuetype)
        {
            if (backby < 0)
            {
                return BadRequest("Only positive integers are allowed");
            }
            SortedDictionary<string, decimal> results = new SortedDictionary<string, decimal>();
            DateTime endDate = DateTime.Now.AddDays(-backby);
            switch (revenuetype.ToLower())
            {
                case "subscription":
                    for (DateTime dt = DateTime.Now.Date; dt >= endDate; dt = dt.AddDays(-1))
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date == dt.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(dt.ToShortDateString(), de);
                    }
                    break;
                case "course":
                    for (DateTime dt = DateTime.Now.Date; dt >= endDate; dt = dt.AddDays(-1))
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date == dt.Date && pl.Description.Contains("course purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(dt.ToShortDateString(), de);
                    }
                    break;
                case "class":
                    for (DateTime dt = DateTime.Now.Date; dt >= endDate; dt = dt.AddDays(-1))
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date == dt.Date && pl.Description.Contains("class purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(dt.ToShortDateString(), de);
                    }
                    break;
                default:
                    return BadRequest("Invalid revenuetype");

            }
            return results;
        }
        //[HttpGet]
        //[Route("[action]/{to}")]
        //public ActionResult SendMail(string to)
        //{
        //    try
        //    {
        //        MailMessage message = new MailMessage();
        //        SmtpClient smtp = new SmtpClient();
        //        message.From = new MailAddress("noreply@smelms.com");
        //        message.To.Add(new MailAddress(to));

        //        message.Subject = "Test";
        //        message.IsBodyHtml = true;
        //        message.Body = "Hello";
        //        smtp.Port = 587;
        //        smtp.Host = "smtp.gmail.com";
        //        smtp.EnableSsl = true;
        //        smtp.UseDefaultCredentials = false;
        //        smtp.Credentials = new NetworkCredential("smeupturnlms@gmail.com", "gpclbvidhudsfrit");
               
        //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        smtp.Send(message);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Error: " + ex);
        //    }
        //}

        [HttpGet]
        [Route("[action]/{backby}/{revenuetype}")]
        public ActionResult<SortedDictionary<string, decimal>> MonthlyRevenue(int backby, string revenuetype)
        {
            if (backby < 0)
            {
                return BadRequest("Only positive integers are allowed");
            }
            SortedDictionary<string, decimal> results = new SortedDictionary<string, decimal>();
            DateTime endMonth = DateTime.Now.AddMonths(-backby);
            switch (revenuetype.ToLower())
            {
                case "subscription":
                    for (DateTime mt = DateTime.Now; mt >= endMonth; mt = mt.AddMonths(-1))
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Month == mt.Month && pl.Paymentdate.Value.Year == mt.Year && pl.Description.Contains("subscription") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(mt.Month + "/" + mt.Year, de);
                    }
                    break;
                case "course":
                    for (DateTime mt = DateTime.Now; mt >= endMonth; mt = mt.AddMonths(-1))
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Month == mt.Month && pl.Paymentdate.Value.Year == mt.Year && pl.Description.Contains("course purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(mt.Month + "/" + mt.Year, de);
                    }
                    break;
                case "class":
                    for (DateTime mt = DateTime.Now; mt >= endMonth; mt = mt.AddMonths(-1))
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Month == mt.Month && pl.Paymentdate.Value.Year == mt.Year && pl.Description.Contains("class purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(mt.Month + "/" + mt.Year, de);
                    }


                    break;
                default:
                    return BadRequest("Invalid revenuetype");

            }
            return results;
        }

        [HttpGet]
        [Route("[action]/{backby}/{revenuetype}")]
        public ActionResult<SortedDictionary<string, decimal>> AnnualRevenue(int backby, string revenuetype)
        {
            if (backby < 0)
            {
                return BadRequest("Only positive integers are allowed");
            }
            SortedDictionary<string, decimal> results = new SortedDictionary<string, decimal>();
            int endYear = DateTime.Now.AddYears(-backby).Year;
            switch (revenuetype.ToLower())
            {
                case "subscription":
                    for (int yr = DateTime.Now.Year; yr >= endYear; yr--)
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Year == yr && pl.Description.Contains("subscription") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(yr.ToString(), de);
                    }
                    break;
                case "course":
                    for (int yr = DateTime.Now.Year; yr >= endYear; yr--)
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Year == yr && pl.Description.Contains("course purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(yr.ToString(), de);
                    }
                    break;
                case "class":
                    for (int yr = DateTime.Now.Year; yr >= endYear; yr--)
                    {
                        decimal de = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Paymentdate.Value.Year == yr && pl.Description.Contains("class purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
                        results.Add(yr.ToString(), de);
                    }
                    break;
                default:
                    return BadRequest("Invalid revenuetype");

            }
            return results;
        }
        private class resClass
        {
            public string Coursecode { set; get; }
            public string Course { set; get; }
            public int Enrollment { set; get; }
            public decimal Earning { set; get; }
            public string Thumbnail { set; get; }

        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult TopSellingCourses()
        {
            List<resClass> feedback = new List<resClass>();
            var response = _context.Customerclass
                            .GroupBy(x => x.Coursecode)
                            .Select(x => new { x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).Take(3).ToList();

            foreach (var e in response)
            {
                var course = _context.Course.SingleOrDefault(c => c.Coursecode == e.Key);
                var earn = _context.Paymentlog.Where(pl => pl.Itemref == e.Key).Sum(pl => pl.Cashamount);

                var res = new resClass()
                {
                    Coursecode = e.Key,
                    Course = course.Title,
                    Enrollment = e.Count,
                    Earning = earn
                };
                var folderName = Path.Combine("Res", "CourseThumbnails");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var fullPath = Path.Combine(pathToSave, res.Coursecode.ToLower() + ".png");
                if (System.IO.File.Exists(fullPath))
                {
                    res.Thumbnail = Path.Combine(folderName, res.Coursecode.ToLower() + ".png");
                }
                feedback.Add(res);
            }
            return Ok(feedback);

        }
    }
}
