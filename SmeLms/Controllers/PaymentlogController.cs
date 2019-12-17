using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
using System.Collections;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentlogController : ControllerBase
    {
        smelmsContext _context;
        public PaymentlogController(smelmsContext _context)
        {
            this._context = _context;
        }
        private class AdvPaymentLog : Paymentlog
        {
            public string cycle { set; get; }
        }


        [Route("[action]/{type}")]
        [HttpGet]
        public async Task<ActionResult> GetByType(string type, int pageNo = 1, int pageSize = 10)
        {
            if (type.ToLower() != "subscription" && type.ToLower() != "purchase")
            {
                return BadRequest("Invalid payment type");
            }
            try
            {
                if (type.ToLower() == "subscription")
                {
                    List<AdvPaymentLog> responses = new List<AdvPaymentLog>();

                    var paylogs = await _context.Paymentlog.Where(pl => pl.Description.Contains("subscription")).ToListAsync();
                    var subid = paylogs.Select(pl => pl.Itemref).ToList();
                    var paysubs = _context.Subscriptionplan.Where(sub => subid.Contains(sub.Subid.ToString()));
                    foreach (Paymentlog pl in paylogs)
                    {
                        var target = paysubs.SingleOrDefault(s => s.Subid.ToString() == pl.Itemref);
                        var obj = new AdvPaymentLog()
                        {
                            Refno = pl.Refno,
                            Customer = pl.Customer,
                            Description = pl.Description,
                            Paymentmode = pl.Paymentmode,
                            Paymentdate = pl.Paymentdate,
                            Cardnumber = pl.Cardnumber,
                            Status = pl.Status,
                            Cashamount = pl.Cashamount,
                            Loyalitypoints = pl.Loyalitypoints,
                            Itemref = pl.Itemref,
                            Itemdescription = pl.Itemdescription,
                            cycle = target.Cycle

                        };
                        responses.Add(obj);
                    }
                    int skip = (pageNo - 1) * pageSize;
                    long total = responses.Count();
                    var record = responses.OrderByDescending(r => r.Paymentdate).Skip(skip).Take(pageSize);
                    return Ok(new PagedResult<AdvPaymentLog>(record, pageNo, pageSize, total));

                }
                else
                {
                    var paylogs = await _context.Paymentlog.Where(pl => !pl.Description.Contains("subscription")).ToListAsync();
                    int skip = (pageNo - 1) * pageSize;
                    long total = paylogs.Count();
                    var record = paylogs.OrderByDescending(r => r.Paymentdate).Skip(skip).Take(pageSize);
                    return Ok(new PagedResult<Paymentlog>(record, pageNo, pageSize, total));

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }

        }
        static decimal decify(object raw)
        {
            return Convert.ToDecimal(raw);
        }
        [HttpGet]
        [Route("[action]")]
        public ActionResult EarningsSummary()
        {
            Hashtable response = new Hashtable();
            response["subscription"] = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Description.Contains("subscription") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
            response["course"] = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Description.Contains("course purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
            response["class"] = Convert.ToDecimal(_context.Paymentlog.Where(pl => pl.Description.Contains("class purchase") && pl.Status == "Accepted").Sum(pl => pl.Cashamount));
            response["gross"] = decify(response["subscription"]) + decify(response["course"]) + decify(response["class"]);
            return Ok(response);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetPurchaseTotal()
        {
            Hashtable response = new Hashtable();
            var logs = await _context.Paymentlog.Where(pl => !pl.Description.Contains("subscription")).ToListAsync();
            response.Add("totalsales", logs.Count());
            response.Add("totalearning", logs.Sum(pl => pl.Cashamount));
            return Ok(response);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetSubscriptionTotal()
        {
            Hashtable response = new Hashtable();
            var logs = await _context.Paymentlog.Where(pl => pl.Description.Contains("subscription")).ToListAsync();
            response.Add("totalearning", logs.Sum(pl => pl.Cashamount));
            return Ok(response);
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<Paymentlog[]>> MyPayments(string email)
        {
            return await _context.Paymentlog.Where(obj => obj.Customer == email).ToArrayAsync();
        }

        [Route("[action]/{status}")]
        [HttpGet]
        public async Task<ActionResult<Paymentlog[]>> GetByStatus(string status)
        {
            return await _context.Paymentlog.Where(obj => obj.Status == status).ToArrayAsync();
        }

        [HttpGet("{Refno}")]
        public async Task<ActionResult<Paymentlog>> Get(string Refno)
        {
            var target = await _context.Paymentlog.SingleOrDefaultAsync(obj => obj.Refno == Refno);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [HttpPost]
        public async Task<ActionResult<Paymentlog>> Post([FromBody] Paymentlog obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid state");
            }
            else
            {
                var cus = await _context.Customer.SingleOrDefaultAsync(c => c.Email == obj.Customer);
                if (cus == null)
                {
                    return BadRequest("Customer doesn't exist");
                }
                if (cus.Loyalitypoint < obj.Loyalitypoints)
                {
                    return BadRequest("Insufficent loyaltypoint balance");
                }
                _context.Paymentlog.Add(obj);
                cus.Loyalitypoint -= obj.Loyalitypoints;
                await _context.SaveChangesAsync();
                return Created("api/Paymentlog", obj);
            }
        }

        /*  [HttpPut ("{Refno}")]
         public async Task<ActionResult> Put (string Refno, [FromBody] Paymentlog obj) {
             var target = await _context.Paymentlog.SingleOrDefaultAsync (nobj => nobj.Refno == Refno);
             if (target != null && ModelState.IsValid) {
                 _context.Entry (target).CurrentValues.SetValues (obj);
                 await _context.SaveChangesAsync ();
                 return Ok ();
             }
             return BadRequest ();
         }*/
        [Route("[action]/{from}/{to}")]
        [HttpGet]
        public async Task<ActionResult<Paymentlog[]>> SalesByDate(DateTime from, DateTime to)
        {
            return await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("purchase") && pl.Status == "Accepted").OrderBy(pl => pl.Paymentdate).ToArrayAsync();

        }

        [Route("[action]/{from}/{to}")]
        [HttpGet]
        public async Task<ActionResult<Paymentlog[]>> EarningsByDate( DateTime from, DateTime to, int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Status == "Accepted").Count();
            var records = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date &&  pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).OrderByDescending(pl => pl.Paymentdate).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Paymentlog>(records, pageNo, pageSize, total));
            //switch (type.ToLower())
            //{
            //    case "subscription":

            //        int skip = (pageNo - 1) * pageSize;
            //        int total = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").Count();
            //        var records = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).OrderByDescending(pl => pl.Paymentdate).Skip(skip).Take(pageSize).ToListAsync();
            //        return Ok(new PagedResult<Paymentlog>(records, pageNo, pageSize, total));

            //    case "course":
            //        int skip2 = (pageNo - 1) * pageSize;
            //        int total2 = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("course purchase") && pl.Status == "Accepted").Count();
            //        var records2 = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("course purchase") && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).Skip(skip2).Take(pageSize).ToListAsync();
            //        return Ok(new PagedResult<Paymentlog>(records2, pageNo, pageSize, total2));

            //    case "class":
            //        int skip3 = (pageNo - 1) * pageSize;
            //        int total3 = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("class purchase") && pl.Status == "Accepted").Count();
            //        var records3 = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("class purchase") && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).ToListAsync();
            //        return Ok(new PagedResult<Paymentlog>(records3, pageNo, pageSize, total3));

            //    default:
            //        return BadRequest("Invalid revenue type");

            //}


        }

        [Route("[action]/{type}/{from}/{to}")]
        [HttpGet]
        public async Task<ActionResult> DateEarningByType(string type,DateTime from, DateTime to, int pageNo = 1, int pageSize = 10)
        {
            //int skip = (pageNo - 1) * pageSize;
            //int total = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Status == "Accepted").Count();
            //var records = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).OrderByDescending(pl => pl.Paymentdate).Skip(skip).Take(pageSize).ToListAsync();
            //return Ok(new PagedResult<Paymentlog>(records, pageNo, pageSize, total));
            switch (type.ToLower())
            {
                case "subscription":

                    //int skip = (pageNo - 1) * pageSize;
                    //int total = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").Count();
                    //var records = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).OrderByDescending(pl => pl.Paymentdate).Skip(skip).Take(pageSize).ToListAsync();
                    return Ok(await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").ToListAsync());

                case "course":
                    //int skip2 = (pageNo - 1) * pageSize;
                    //int total2 = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("course purchase") && pl.Status == "Accepted").Count();
                    //var records2 = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("course purchase") && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).Skip(skip2).Take(pageSize).ToListAsync();
                    return Ok(await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("course purchase") && pl.Status == "Accepted").ToListAsync());

                case "class":
                    //int skip3 = (pageNo - 1) * pageSize;
                    //int total3 = _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("class purchase") && pl.Status == "Accepted").Count();
                    //var records3 = await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("class purchase") && pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).ToListAsync();
                    return Ok(await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("class purchase") && pl.Status == "Accepted").ToListAsync());

                default:
                    return BadRequest("Invalid revenue type");

            }


        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<Paymentlog[]>> AllEarnings(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Paymentlog.Where(pl =>  pl.Status == "Accepted").Count();
            var records = await _context.Paymentlog.Where(pl => pl.Status == "Accepted").OrderByDescending(pl => pl.Paymentdate).OrderByDescending(pl => pl.Paymentdate).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Paymentlog>(records, pageNo, pageSize, total));

        }

        [Route("[action]/{from}/{to}")]
        [HttpGet]
        public async Task<ActionResult<Paymentlog[]>> SubByDate(DateTime from, DateTime to)
        {
            return await _context.Paymentlog.Where(pl => pl.Paymentdate.Value.Date >= from.Date && pl.Paymentdate.Value.Date <= to.Date && pl.Description.Contains("subscription") && pl.Status == "Accepted").OrderBy(pl => pl.Paymentdate).ToArrayAsync();

        }
    }
}