using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
using System.IO;
using System.Collections;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InclassController : ControllerBase
    {
        smelmsContext _context;
        public InclassController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult> Get(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            var classes = await _context.Inclass.ToListAsync();
            long total = classes.Count();
            var records = classes.OrderBy(r => r.Title).Skip(skip).Take(pageSize);
            return Ok(new PagedResult<Inclass>(records, pageNo, pageSize, total));
        }

        [Route("[action]/{classid}")]
        [HttpGet]
        public ActionResult GetExtras(int classid)
        {
            Hashtable response = new Hashtable();
            response.Add("enrolled", _context.Inclassregistration.Where(r => r.Classid == classid).Count());
            response.Add("sales", _context.Paymentlog.Where(p => p.Itemref == classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount));
            var folderName = Path.Combine("Res", "ClassThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, classid + ".png");
            if (System.IO.File.Exists(fullPath))
            {
                response.Add("thumbnail", Path.Combine(folderName, classid + ".png"));
            }
            return Ok(response);

        }

        [Route("[action]/{classid}")]
        [HttpGet]
        public async Task<ActionResult<Customer[]>> EnrolledFor(int classid)
        {
            var estudents = await _context.Inclassregistration.Where(obj => obj.Classid == classid).Select(obj => obj.Email).ToListAsync();
            return await _context.Customer.Where(c => estudents.Contains(c.Email)).ToArrayAsync();

        }

        [Route("[action]/{user}")]
        [HttpGet]
        public async Task<ActionResult<Inclass[]>> CreatedBy(string user)
        {
            return await _context.Inclass.Where(obj => obj.Createdby == user).ToArrayAsync();
        }

        [Route("[action]/{searchtext}")]
        [HttpGet]
        public async Task<ActionResult<Inclass[]>> Search(string searchtext)
        {
            return await _context.Inclass.Where(obj => obj.Title.Contains(searchtext)).ToArrayAsync();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<Inclass[]>> GetUpcoming(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Inclass.Where(obj => DateTime.Now.Date <= obj.Startdate.Date && obj.Status.ToLower() != "suspended").Count();
            var records = await _context.Inclass.Where(obj => DateTime.Now.Date <= obj.Startdate.Date && obj.Status.ToLower() != "suspended").OrderBy(c => c.Title).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Inclass>(records, pageNo, pageSize, total));
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetSuspended(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Inclass.Where(obj => obj.Status.ToLower() == "suspended").Count();
            var records = await _context.Inclass.Where(obj => obj.Status.ToLower() == "suspended").ToListAsync();
            return Ok(new PagedResult<Inclass>(records, pageNo, pageSize, total));
        }
        private class AthClass : Inclass
        {
            public string Authorname { set; get; }
            public AthClass(Inclass obj)
            {
                this.Catid = obj.Catid;
                this.Classid = obj.Classid;
                this.Coursedescription = obj.Coursedescription;
                this.Createdby = obj.Createdby;
                this.Duration = obj.Duration;
                this.Enddate = obj.Enddate;
                this.Location = obj.Location;
                this.Loyalitypoint = obj.Loyalitypoint;
                this.Objectives = obj.Objectives;
                this.Price = obj.Price;
                this.Startdate = obj.Startdate;
                this.Starttime = obj.Starttime;
                this.Status = obj.Status;
                this.Timedescription = obj.Timedescription;
                this.Title = obj.Title;

            }
        }
        private class AdvClass : Inclass
        {
            public int Enrolled { set; get; }
            public decimal Sales { set; get; }
            public AdvClass(Inclass obj)
            {
                this.Catid = obj.Catid;
                this.Classid = obj.Classid;
                this.Coursedescription = obj.Coursedescription;
                this.Createdby = obj.Createdby;
                this.Duration = obj.Duration;
                this.Enddate = obj.Enddate;
                this.Location = obj.Location;
                this.Loyalitypoint = obj.Loyalitypoint;
                this.Objectives = obj.Objectives;
                this.Price = obj.Price;
                this.Startdate = obj.Startdate;
                this.Starttime = obj.Starttime;
                this.Status = obj.Status;
                this.Timedescription = obj.Timedescription;
                this.Title = obj.Title;
            }
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortByEnrolled(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            if (filtertype == null)
            {
                if (filtervalue != null)
                {
                    return BadRequest("Filter type is required");
                }
                var allClasses = await _context.Inclass.ToListAsync();
                List<AdvClass> advClasses = new List<AdvClass>();
                foreach (Inclass c in allClasses)
                {
                    AdvClass advc = new AdvClass(c);
                    advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advClasses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advClasses.Count();
                var response = advClasses.OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advClasses.OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);

                }
                if (author != null)
                {
                    total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                    response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                    }
                }
                return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
            }
            else
            {
                if (filtervalue == null)
                {
                    return BadRequest("Filter value cannot be null");
                }
                switch (filtertype.ToLower())
                {
                    case "status":
                        List<string> status = new List<string>(){
                                "suspended","completed","upcoming","ongoing"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid class status passed as filter value");
                        }

                        var allClasses = await _context.Inclass.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvClass> advClasses = new List<AdvClass>();
                        foreach (Inclass c in allClasses)
                        {
                            AdvClass advc = new AdvClass(c);
                            advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advClasses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advClasses.Count();
                        var response = advClasses.OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advClasses.OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                            response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                            if (mode != "asc")
                            {
                                response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
                    default:
                        return BadRequest("Invalid filter type");
                }



            }
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortAlphabetically(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            if (filtertype == null)
            {
                if (filtervalue != null)
                {
                    return BadRequest("Filter type is required");
                }
                var allClasses = await _context.Inclass.ToListAsync();
                List<AdvClass> advClasses = new List<AdvClass>();
                foreach (Inclass c in allClasses)
                {
                    AdvClass advc = new AdvClass(c);
                    advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advClasses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advClasses.Count();
                var response = advClasses.OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advClasses.OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                    response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                    }
                }
                return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
            }
            else
            {
                if (filtervalue == null)
                {
                    return BadRequest("Filter value cannot be null");
                }
                switch (filtertype.ToLower())
                {
                    case "status":
                        List<string> status = new List<string>(){
                                "suspended","completed","upcoming","ongoing"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid class status passed as filter value");
                        }

                        var allClasses = await _context.Inclass.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvClass> advClasses = new List<AdvClass>();
                        foreach (Inclass c in allClasses)
                        {
                            AdvClass advc = new AdvClass(c);
                            advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advClasses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advClasses.Count();
                        var response = advClasses.OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advClasses.OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                            response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                            if (mode != "asc")
                            {
                                response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
                    default:
                        return BadRequest("Invalid filter type");
                }



            }
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortBySales(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            if (filtertype == null)
            {
                if (filtervalue != null)
                {
                    return BadRequest("Filter type is required");
                }
                var allClasses = await _context.Inclass.ToListAsync();
                List<AdvClass> advClasses = new List<AdvClass>();
                foreach (Inclass c in allClasses)
                {
                    AdvClass advc = new AdvClass(c);
                    advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advClasses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advClasses.Count();
                var response = advClasses.OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advClasses.OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                    response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                    }

                }
                return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
            }
            else
            {
                if (filtervalue == null)
                {
                    return BadRequest("Filter value cannot be null");
                }
                switch (filtertype.ToLower())
                {
                    case "status":
                        List<string> status = new List<string>(){
                                "suspended","completed","upcoming","ongoing"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid class status passed as filter value");
                        }

                        var allClasses = await _context.Inclass.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvClass> advClasses = new List<AdvClass>();
                        foreach (Inclass c in allClasses)
                        {
                            AdvClass advc = new AdvClass(c);
                            advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advClasses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advClasses.Count();
                        var response = advClasses.OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advClasses.OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                            response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                            if (mode != "asc") {
                                response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
                    default:
                        return BadRequest("Invalid filter type");
                }

            }
        }


        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortByStartDate(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            if (filtertype == null)
            {
                if (filtervalue != null)
                {
                    return BadRequest("Filter type is required");
                }
                var allClasses = await _context.Inclass.ToListAsync();
                List<AdvClass> advClasses = new List<AdvClass>();
                foreach (Inclass c in allClasses)
                {
                    AdvClass advc = new AdvClass(c);
                    advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advClasses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advClasses.Count();
                var response = advClasses.OrderBy(c => c.Startdate).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advClasses.OrderByDescending(c => c.Startdate).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                    response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Startdate).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Startdate).Skip(skip).Take(pageSize);
                    }
                }
                return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
            }
            else
            {
                if (filtervalue == null)
                {
                    return BadRequest("Filter value cannot be null");
                }
                switch (filtertype.ToLower())
                {
                    case "status":
                        List<string> status = new List<string>(){
                                "suspended","completed","upcoming","ongoing"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid class status passed as filter value");
                        }

                        var allClasses = await _context.Inclass.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvClass> advClasses = new List<AdvClass>();
                        foreach (Inclass c in allClasses)
                        {
                            AdvClass advc = new AdvClass(c);
                            advc.Enrolled = _context.Inclassregistration.Where(r => r.Classid == c.Classid).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Classid.ToString() && p.Description.Contains("class purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advClasses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advClasses.Count();
                        var response = advClasses.OrderBy(c => c.Startdate).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advClasses.OrderByDescending(c => c.Startdate).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).Count();
                            response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderBy(c => c.Startdate).Skip(skip).Take(pageSize);
                            if (mode != "asc")
                            {
                                response = advClasses.Where(c => c.Createdby.ToLower() == author.ToLower()).OrderByDescending(c => c.Startdate).Skip(skip).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvClass>(response, pageNo, pageSize, total));
                    default:
                        return BadRequest("Invalid filter type");
                }



            }
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var classes = await _context.Inclass.ToListAsync();
            List<Inclass> response = new List<Inclass>();
            foreach (Inclass c in classes)
            {
                var ac = new AthClass(c);
                var author = _context.Users.SingleOrDefault(u => u.Email == c.Createdby);
                if (author != null)
                {
                    ac.Authorname = author.Firstname + " " + author.Lastname;
                }
                response.Add(ac);

            }
            return Ok(response.OrderBy(c => c.Title));
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetCompleted(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Inclass.Where(obj => DateTime.Now.Date > obj.Enddate.Date && obj.Status.ToLower() != "suspended").Count();
            var records = await _context.Inclass.Where(obj => DateTime.Now.Date > obj.Enddate.Date && obj.Status.ToLower() != "suspended").OrderBy(r => r.Title).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Inclass>(records, pageNo, pageSize, total));
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> AutoChangeStatus()
        {

            var dues = await _context.Inclass.Where(obj => DateTime.Now.Date > obj.Enddate.Date && obj.Status.ToLower() != "suspended").ToListAsync();
            foreach (var c in dues)
            {
                c.Status = "Completed";
            }
            var started = await _context.Inclass.Where(obj => DateTime.Now.Date > obj.Startdate.Date && DateTime.Now.Date < obj.Enddate.Date && obj.Status.ToLower() != "suspended").ToListAsync();
            foreach (var c in started)
            {
                c.Status = "Ongoing";
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{classid}")]
        public async Task<ActionResult<Inclass>> Get(int classid)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [Route("[action]/{catid}")]
        [HttpGet]
        public async Task<ActionResult<Inclass[]>> GetByCat(string catid)
        {
            var target = await _context.Coursecategory.SingleOrDefaultAsync(obj => obj.Catid == Convert.ToInt32(catid));
            if (target == null)
            {
                return NotFound();
            }
            return await _context.Inclass.Where(obj => obj.Catid.Contains(catid)).ToArrayAsync();
        }

        [Route("[action]/{month}/{year}")]
        [HttpGet]
        public async Task<ActionResult<Inclass[]>> GetByMonth(int month, int year)
        {

            return await _context.Inclass.Where(obj => obj.Startdate.Month == month && obj.Startdate.Year == year && obj.Status.ToLower() != "suspended").ToArrayAsync();
        }

        [Route("[action]/{classid}")]
        [HttpPost]
        public async Task<ActionResult> Unsuspend(int classid)
        {

            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target != null)
            {
                if (DateTime.Now.Date <= target.Enddate.Date)
                {
                    target.Status = "Upcoming";
                }
                else
                {
                    target.Status = "Completed";
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Inclass>> Post([FromBody] Inclass obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }
            else
            {
                _context.Inclass.Add(obj);
                await _context.SaveChangesAsync();
                return Created("api/Inclass", obj);
            }
        }

        [HttpPut("{classid}")]
        public async Task<ActionResult> Put(int classid, [FromBody] Inclass obj)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(nobj => nobj.Classid == classid);
            if (target != null && ModelState.IsValid)
            {
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{classid}")]
        public async Task<ActionResult> Delete(int classid)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target != null)
            {
                target.Status = "Suspended";
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [Route("[action]/{classid}")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadThumbnail(int classid)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target == null || Request.Form.Files.Count == 0)
            {
                return BadRequest("Invalid classid or no file in request body");
            }
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "ClassThumbnails");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, classid + ".png");
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [Route("[action]/{classid}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetThumbnail(int classid)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target == null)
            {
                return BadRequest("Invalid classid");
            }
            var folderName = Path.Combine("Res", "ClassThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, classid + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return Path.Combine(folderName, classid + ".png");
        }

        [Route("[action]/{classid}")]
        [HttpDelete]
        public async Task<ActionResult> RemoveThumbnail(int classid)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target == null)
            {
                return BadRequest("Invalid classid");
            }
            var folderName = Path.Combine("Res", "ClassThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, classid + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return BadRequest("No thumbnail found for this class");
            }

            System.IO.File.Delete(Path.Combine(folderName, classid + ".png"));
            return Ok();
        }

        [Route("[action]/{classid}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetThumbnailRaw(int classid)
        {
            var target = await _context.Inclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target == null)
            {
                return BadRequest("Invalid classid");
            }
            var folderName = Path.Combine("Res", "ClassThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, classid + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return await System.IO.File.ReadAllBytesAsync(Path.Combine(folderName, classid + ".png").ToString());
        }
    }
}