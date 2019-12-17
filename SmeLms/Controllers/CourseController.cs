using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
using System.Collections;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        smelmsContext _context;
        public CourseController(smelmsContext _context)
        {
            this._context = _context;
        }
        string email, subject;
        string[] bccs;
        [HttpGet]
        public async Task<ActionResult> Get(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            var courses = await _context.Course.ToListAsync();
            long total = courses.Count();
            var records = courses.OrderBy(c => c.Title).Skip(skip).Take(pageSize);
            return Ok(new PagedResult<Course>(records, pageNo, pageSize, total));
        }
        private class AthCourse : Course
        {
            public AthCourse(Course obj)
            {
                this.Author = obj.Author;
                this.Catid = obj.Catid;
                this.Coursecode = obj.Coursecode;
                this.Description = obj.Description;
                this.Duration = obj.Duration;
                this.Enforcesequence = obj.Enforcesequence;
                this.Freeonsubcription = obj.Freeonsubcription;
                this.Loyaltypoint = obj.Loyaltypoint;
                this.Objectives = obj.Objectives;
                this.Price = obj.Price;
                this.Publisheddate = obj.Publisheddate;
                this.Relatedcourses = obj.Relatedcourses;
                this.Showstudentcount = obj.Showstudentcount;
                this.Status = obj.Status;
                this.Tag = obj.Tag;
                this.Title = obj.Title;

            }
            public string Authorname { set; get; }
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var allCourses = await _context.Course.ToListAsync();
            List<AthCourse> response = new List<AthCourse>();
            foreach (Course c in allCourses)
            {
                AthCourse ac = new AthCourse(c);
                var ath = await _context.Users.SingleOrDefaultAsync(u => u.Email == c.Author);
                if (ath != null)
                {
                    ac.Authorname = ath.Firstname + " " + ath.Lastname;
                }
                response.Add(ac);
            }
            return Ok(response.OrderBy(c => c.Title));
        }

        [HttpGet("{coursecode}")]
        public async Task<ActionResult<Course>> Get(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [Route("[action]/{title}")]
        [HttpGet]
        public async Task<ActionResult> SearchByTitle(string title)
        {
            return Ok(await _context.Course.Where(c => c.Title.Contains(title)).ToListAsync());
        }
        private class AdvCourse : Course
        {
            public int Enrolled { set; get; }
            public decimal Sales { set; get; }
            public AdvCourse(Course obj)
            {
                this.Author = obj.Author;
                this.Catid = obj.Catid;
                this.Coursecode = obj.Coursecode;
                this.Description = obj.Description;
                this.Duration = obj.Duration;
                this.Enforcesequence = obj.Enforcesequence;
                this.Freeonsubcription = obj.Freeonsubcription;
                this.Loyaltypoint = obj.Loyaltypoint;
                this.Objectives = obj.Objectives;
                this.Price = obj.Price;
                this.Publisheddate = obj.Publisheddate;
                this.Relatedcourses = obj.Relatedcourses;
                this.Showstudentcount = obj.Showstudentcount;
                this.Status = obj.Status;
                this.Tag = obj.Tag;
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
                var allCourses = await _context.Course.ToListAsync();
                List<AdvCourse> advCourses = new List<AdvCourse>();
                foreach (Course c in allCourses)
                {
                    AdvCourse advc = new AdvCourse(c);
                    advc.Enrolled = _context.Customerclass.Where(r => r.Coursecode == c.Coursecode).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advCourses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advCourses.Count();
                var response = advCourses.OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advCourses.OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = advCourses.Where(cl => cl.Author.ToLower() == author.ToLower()).Count();
                    response = advCourses.Where(cl => cl.Author.ToLower() == author.ToLower()).OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advCourses.Where(cl => cl.Author.ToLower() == author.ToLower()).OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                    }
                }
                return Ok(new PagedResult<AdvCourse>(response, pageNo, pageSize, total));
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
                                "indraft","published","suspended"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid course status passed as filter value");
                        }

                        var allCourses = await _context.Course.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvCourse> advCourses = new List<AdvCourse>();
                        foreach (Course c in allCourses)
                        {
                            AdvCourse advc = new AdvCourse(c);
                            advc.Enrolled = _context.Customerclass.Where(r => r.Coursecode == c.Coursecode).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advCourses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advCourses.Count();
                        var response = advCourses.OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advCourses.OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advCourses.Where(cl => cl.Author.ToLower() == author.ToLower()).Count();
                            response = advCourses.Where(cl => cl.Author.ToLower() == author.ToLower()).OrderBy(c => c.Enrolled).Skip(skip).Take(pageSize);
                            if (mode != "asc") {
                                response = advCourses.Where(cl => cl.Author.ToLower() == author.ToLower()).OrderByDescending(c => c.Enrolled).Skip(skip).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvCourse>(response, pageNo, pageSize, total));
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
                var allCourses = await _context.Course.ToListAsync();
                List<AdvCourse> advCourses = new List<AdvCourse>();
                foreach (Course c in allCourses)
                {
                    AdvCourse advc = new AdvCourse(c);
                    advc.Enrolled = _context.Customerclass.Where(r => r.Coursecode == c.Coursecode).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advCourses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advCourses.Count();
                var response = advCourses.OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advCourses.OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).Count();
                    response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                    }

                }

                return Ok(new PagedResult<AdvCourse>(response, pageNo, pageSize, total));
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
                                "indraft","published","suspended"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid course status passed as filter value");
                        }

                        var allCourses = await _context.Course.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvCourse> advCourses = new List<AdvCourse>();
                        foreach (Course c in allCourses)
                        {
                            AdvCourse advc = new AdvCourse(c);
                            advc.Enrolled = _context.Customerclass.Where(r => r.Coursecode == c.Coursecode).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advCourses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advCourses.Count();
                        var response = advCourses.OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advCourses.OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).Count();
                            response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderBy(c => c.Title).Skip(skip).Take(pageSize);
                            if (mode != "asc")
                            {
                                advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderByDescending(c => c.Title).Skip(skip).Take(pageSize);
                            }

                        }
                        return Ok(new PagedResult<AdvCourse>(response, pageNo, pageSize, total));
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
                var allCourses = await _context.Course.ToListAsync();
                List<AdvCourse> advCourses = new List<AdvCourse>();
                foreach (Course c in allCourses)
                {
                    AdvCourse advc = new AdvCourse(c);
                    advc.Enrolled = _context.Customerclass.Where(r => r.Coursecode == c.Coursecode).Count();
                    advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                    advCourses.Add(advc);
                }
                int skip = (pageNo - 1) * pageSize;
                int total = advCourses.Count();
                var response = advCourses.OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advCourses.OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).Count();
                    response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                    }
                }
                return Ok(new PagedResult<AdvCourse>(response, pageNo, pageSize, total));
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
                                "indraft","published","suspended"
                        };
                        if (!status.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid course status passed as filter value");
                        }

                        var allCourses = await _context.Course.Where(c => c.Status == filtervalue).ToListAsync();
                        List<AdvCourse> advCourses = new List<AdvCourse>();
                        foreach (Course c in allCourses)
                        {
                            AdvCourse advc = new AdvCourse(c);
                            advc.Enrolled = _context.Customerclass.Where(r => r.Coursecode == c.Coursecode).Count();
                            advc.Sales = _context.Paymentlog.Where(p => p.Itemref == c.Coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount);
                            advCourses.Add(advc);
                        }
                        int skip = (pageNo - 1) * pageSize;
                        int total = advCourses.Count();
                        var response = advCourses.OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                        if (mode != "asc") {
                            response = advCourses.OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).Count();
                            response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderBy(c => c.Sales).Skip(skip).Take(pageSize);
                            if (mode != "asc") {
                                response = advCourses.Where(ac => ac.Author.ToLower() == author.ToLower()).OrderByDescending(c => c.Sales).Skip(skip).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvCourse>(response, pageNo, pageSize, total));
                    default:
                        return BadRequest("Invalid filter type");
                }



            }
        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsPublished(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null)
            {
                return NotFound();
            }
            return target.Status.ToLower() == "published";
        }

        [Route("[action]/{catid}")]
        [HttpGet]
        public async Task<ActionResult<Course[]>> GetByCat(string catid)
        {
            var target = await _context.Coursecategory.SingleOrDefaultAsync(obj => obj.Catid == Convert.ToInt32(catid));
            if (target == null)
            {
                return NotFound();
            }
            return await _context.Course.Where(obj => obj.Catid.Contains(catid)).ToArrayAsync();
        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<Customer[]>> EnrolledFor(string coursecode)
        {
            var estudents = await _context.Customerclass.Where(obj => obj.Coursecode == coursecode).Select(obj => obj.Customer).ToListAsync();
            return await _context.Customer.Where(c => estudents.Contains(c.Email)).ToArrayAsync();

        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<Customer[]>> Completed(string coursecode)
        {
            var estudents = await _context.Customerclass.Where(obj => obj.Coursecode == coursecode && obj.Status == "completed").Select(obj => obj.Customer).ToListAsync();
            return await _context.Customer.Where(c => estudents.Contains(c.Email)).ToArrayAsync();

        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<Customer[]>> NotCompleted(string coursecode)
        {
            var estudents = await _context.Customerclass.Where(obj => obj.Coursecode == coursecode && obj.Status != "completed").Select(obj => obj.Customer).ToListAsync();
            return await _context.Customer.Where(c => estudents.Contains(c.Email)).ToArrayAsync();

        }

        [Route("[action]/{author}")]
        [HttpGet]
        public async Task<ActionResult<Course[]>> GetByAuthor(string author)
        {
            return await _context.Course.Where(obj => obj.Author == author).ToArrayAsync();
        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<Course[]>> GetRelatedCoursesByID(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null)
            {
                return NotFound();
            }
            return await _context.Course.Where(obj => target.Relatedcourses.Contains(obj.Coursecode)).ToArrayAsync();
        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<Course[]>> GetRelatedCoursesByTag(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);

            if (target == null)
            {
                return NotFound();
            }
            List<Course> rcourses = new List<Course>();
            foreach (Course c in _context.Course)
            {
                if (c == target)
                {
                    continue;
                }
                foreach (string t in target.Tag.Split(";"))
                {
                    if (c.Tag.Contains(t))
                    {
                        rcourses.Add(c);
                        break;
                    }
                }
            }

            return rcourses.ToArray();

        }
        private void dispatchMail()
        {
            Util.SendMail(email, subject, bccs);
        }

        [Route("[action]/{author}")]
        [HttpGet]
        public async Task<ActionResult<SearchCourseResult[]>> SortByAuthor(string author, string mode = "asc")
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            var courses = await _context.Course.Where(c => c.Author == author).ToListAsync();
            List<SearchCourseResult> rcourses = new List<SearchCourseResult>();

            foreach (Course c in courses)
            {
                var ntotal = await _context.Customerclass.Where(cc => cc.Coursecode == c.Coursecode).CountAsync();
                var etotal = await _context.Paymentlog.Where(pl => pl.Itemref == c.Coursecode).SumAsync(pl => pl.Cashamount);
                SearchCourseResult scr = new SearchCourseResult()
                {
                    Author = c.Author,
                    Catid = c.Catid,
                    Coursecode = c.Coursecode,
                    Duration = c.Duration,
                    Title = c.Title,
                    Publisheddate = c.Publisheddate,
                    Price = c.Price,
                    Status = c.Status,
                    Loyaltypoint = c.Loyaltypoint,
                    Enforcesequence = c.Enforcesequence,
                    Freeonsubcription = c.Freeonsubcription,
                    Relatedcourses = c.Relatedcourses,
                    Description = c.Description,
                    Tag = c.Tag,
                    Objectives = c.Objectives,
                    Numberofstudents = ntotal,
                    Totalearnings = Convert.ToDecimal(etotal)
                };
                rcourses.Add(scr);
            }
            if (mode != "asc")
            {
                return rcourses.OrderByDescending(rc => rc.Numberofstudents).ToArray();
            }
            return rcourses.OrderBy(rc => rc.Numberofstudents).ToArray();

        }

        [Route("[action]/{searchtext}/{category}/{author}/{status}")]
        [HttpGet]
        public async Task<ActionResult<Course[]>> Search(string searchtext, string category, string author, string status)
        {
            if (category.ToLower() == "all" && author.ToLower() == "all" && status.ToLower() == "all")
            {
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext))).ToArrayAsync();
            }
            if (category.ToLower() != "all" && author.ToLower() == "all" && status.ToLower() == "all")
            {
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Catid.Contains(category)).ToArrayAsync();
            }
            else if (category.ToLower() == "all" && author.ToLower() != "all" && status.ToLower() == "all")
            {

                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Author == author).ToArrayAsync();
            }
            else if (category.ToLower() == "all" && author.ToLower() == "all" && status.ToLower() != "all")
            {
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Status == status).ToArrayAsync();
            }
            else if (category.ToLower() != "all" && author.ToLower() != "all" && status.ToLower() == "all")
            {

                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Catid.Contains(category) && c.Author == author).ToArrayAsync();
            }
            else if (category.ToLower() != "all" && author.ToLower() == "all" && status.ToLower() != "all")
            {
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Catid.Contains(category) && c.Status == status).ToArrayAsync();
            }
            else if (category.ToLower() == "all" && author.ToLower() != "all" && status.ToLower() != "all")
            {
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Author == author && c.Status == status).ToArrayAsync();
            }
            else
            {
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Author == author && c.Status == status && c.Catid.Contains(category)).ToArrayAsync();
            }
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetPublished(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Course.Where(obj => obj.Status.ToLower() == "published").Count();
            var records = await _context.Course.Where(obj => obj.Status.ToLower() == "published").OrderBy(c => c.Title).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Course>(records, pageNo, pageSize, total));
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetSuspended(int pageNo = 1, int pageSize = 1)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Course.Where(obj => obj.Status.ToLower() == "suspended").Count();
            var records = await _context.Course.Where(obj => obj.Status.ToLower() == "suspended").OrderBy(c => c.Title).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Course>(records, pageNo, pageSize, total));
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetIndraft(int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            int total = _context.Course.Where(obj => obj.Status.ToLower() == "indraft").Count();
            var records = await _context.Course.Where(obj => obj.Status.ToLower() == "indraft").OrderBy(c => c.Title).Skip(skip).Take(pageSize).ToListAsync();
            return Ok(new PagedResult<Course>(records, pageNo, pageSize, total));
        }

        [Route("[action]/{coursecode}")]
        [HttpPost]
        public async Task<ActionResult> Publish(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);

            if (target != null)
            {
                target.Status = "Published";
                target.Publisheddate = DateTime.Now;

                var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
                string[] aemails = _context.Users.Where(u => u.Role == "Admin").Select(u => u.Email).ToArray();

                foreach (var u in admins)
                {
                    var note = new Notifications()
                    {
                        Type = "Publish",
                        Target = u.Email,
                        Remark = "This course " + target.Title + " [" + target.Coursecode + "] just got published."
                    };
                    _context.Notifications.Add(note);
                }
                await _context.SaveChangesAsync();
                email = "This course " + target.Title + " [" + target.Coursecode + "] just got published.";
                subject = "Course Publish Alert";
                bccs = aemails;
                ThreadStart ts = new ThreadStart(dispatchMail);
                Thread t1 = new Thread(ts);
                t1.Start();
                //Util.SendMail (email, "Course Publish Alert", aemails);
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Course>> Post([FromBody] Course obj)
        {
            try
            {
                List<string> status = new List<string>()
                {
                    "indraft","suspended","published"
                };
                if (!status.Contains(obj.Status.ToLower()))
                {
                    return BadRequest("Invalid course status");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid model state");
                }

                else
                {
                    if (obj.Coursecode == null)
                    {
                        string id = "";
                        for (int i = 1; i <= 7; i++)
                        {
                            id += new Random().Next(0, 9);
                        }
                        obj.Coursecode = obj.Title.Substring(0, 3) + id;
                    }
                    _context.Course.Add(obj);
                    await _context.SaveChangesAsync();
                    return Created("api/Course", obj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex);
            }
        }

        [HttpPut("{coursecode}")]
        public async Task<ActionResult> Put(string coursecode, [FromBody] Course obj)
        {
            var target = await _context.Course.SingleOrDefaultAsync(nobj => nobj.Coursecode == coursecode);
            if (obj.Catid == null || obj.Catid == "")
            {
                return BadRequest("Invalid Catid");
            }
            if (target != null && ModelState.IsValid)
            {
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{coursecode}")]
        public async Task<ActionResult> Suspend(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);

            if (target != null)
            {
                target.Status = "Suspended";
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [Route("[action]/{coursecode}/{catid}")]
        [HttpGet]
        public async Task<ActionResult<bool>> HasOtherCat(string coursecode, string catid)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            var target2 = await _context.Coursecategory.SingleOrDefaultAsync(obj => obj.Catid == Convert.ToInt32(catid));
            if (target == null || target2 == null)
            {
                return BadRequest();
            }
            return catid != target.Catid;

        }
        [Route("[action]/{coursecode}")]
        [HttpGet]
        public ActionResult GetExtras(string coursecode)
        {
            Hashtable response = new Hashtable();
            response.Add("enrolled", _context.Customerclass.Where(r => r.Coursecode == coursecode).Count());
            response.Add("sales", _context.Paymentlog.Where(p => p.Itemref == coursecode && p.Description.Contains("course purchase") && p.Status == "accepted").Sum(p => p.Cashamount));
            var folderName = Path.Combine("Res", "CourseThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, coursecode.ToLower() + ".png");
            if (System.IO.File.Exists(fullPath))
            {
                response.Add("thumbnail", Path.Combine(folderName, coursecode.ToLower() + ".png"));
            }
            return Ok(response);

        }

        [Route("[action]/{coursecode}")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadThumbnail(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null || Request.Form.Files.Count == 0)
            {
                return BadRequest("Invalid coursecode or no file in request body");
            }
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "CourseThumbnails");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, coursecode.ToLower() + ".png");
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

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetThumbnail(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null)
            {
                return BadRequest("Invalid coursecode");
            }
            var folderName = Path.Combine("Res", "CourseThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, coursecode.ToLower() + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return Path.Combine(folderName, coursecode.ToLower() + ".png");
        }

        [Route("[action]/{coursecode}")]
        [HttpDelete]
        public async Task<ActionResult> RemoveThumbnail(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null)
            {
                return BadRequest("Invalid coursecode");
            }
            var folderName = Path.Combine("Res", "CourseThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, coursecode.ToLower() + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return BadRequest("No thumbnail found for this course");
            }
            System.IO.File.Delete(Path.Combine(folderName, coursecode.ToLower() + ".png"));
            return Ok();
        }

        [Route("[action]/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetThumbnailRaw(string coursecode)
        {
            var target = await _context.Course.SingleOrDefaultAsync(obj => obj.Coursecode == coursecode);
            if (target == null)
            {
                return BadRequest("Invalid coursecode");
            }
            var folderName = Path.Combine("Res", "CourseThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, coursecode.ToLower() + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return await System.IO.File.ReadAllBytesAsync(Path.Combine(folderName, coursecode.ToLower() + ".png").ToString());
        }

        [Route("[action]/{searchtext}/{category}/{minprice}/{maxprice}")]
        [HttpGet]
        public async Task<ActionResult<Course[]>> SearchWithFilter(string searchtext, string category, int minprice, int maxprice)
        {
            if (category.ToLower() == "all" && minprice == 0 && maxprice == 0)
            { // only search text
                return await _context.Course.Where(c => c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext) && c.Status == "published").ToArrayAsync();
            }
            else if (category.ToLower() != "all" && minprice == 0 && maxprice == 0)
            { //search text with cat only
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Catid.Contains(category) && c.Status == "published").ToArrayAsync();
            }
            else if (category.ToLower() == "all" && minprice != 0 && maxprice == 0)
            { //search text with min price only
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Price >= minprice && c.Status == "published").ToArrayAsync();
            }
            else if (category.ToLower() == "all" && minprice == 0 && maxprice != 0)
            { //search text with max price only
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Price <= maxprice && c.Status == "published").ToArrayAsync();
            }
            else if (category.ToLower() == "all" && minprice != 0 && maxprice != 0)
            { //search text with min & max prices
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Price >= minprice && c.Price <= maxprice && c.Status == "published").ToArrayAsync();
            }
            else if (category.ToLower() != "all" && minprice != 0 && maxprice == 0)
            { //search text with cat and min price
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Price >= minprice && c.Status == "published" && c.Catid.Contains(category)).ToArrayAsync();
            }
            else if (category.ToLower() != "all" && minprice == 0 && maxprice != 0)
            { //search text with cat and max price
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Price <= maxprice && c.Status == "published" && c.Catid.Contains(category)).ToArrayAsync();
            }
            else
            { //search text with cat, min and max prices
                return await _context.Course.Where(c => (c.Title.Contains(searchtext) || c.Description.Contains(searchtext) || c.Tag.Contains(searchtext)) && c.Price >= minprice && c.Price <= maxprice && c.Status == "published" && c.Catid.Contains(category)).ToArrayAsync();
            }

        }

    }
}