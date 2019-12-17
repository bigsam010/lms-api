using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomercourseController : ControllerBase
    {
        smelmsContext _context;
        public CustomercourseController(smelmsContext _context)
        {
            this._context = _context;
        }
        string email, subject;
        string[] bccs;

        string email2, subject2;
        string[] bccs2;

        string email3, subject3;
        string[] bccs3;
        void dispatchMail()
        {
            Util.SendMail(email, subject, bccs);
        }

        void dispatchMail2()
        {
            Util.SendMail(email2, subject2, bccs2);
        }

        void dispatchMail3()
        {
            Util.SendMail(email3, subject3, bccs3);
        }

        [HttpGet]
        public async Task<ActionResult<Customerclass[]>> Get()
        {
            return await _context.Customerclass.ToArrayAsync();
        }

        [HttpGet("{classid}")]
        public async Task<ActionResult<Customerclass>> Get(int classid)
        {
            var target = await _context.Customerclass.SingleOrDefaultAsync(obj => obj.Classid == classid);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [Route("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Customerclass[]>> AllCourses(string customer)
        {

            return await _context.Customerclass.Where(obj => obj.Customer == customer).ToArrayAsync();

        }

        [Route("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Customerclass[]>> SelectedCourses(string customer)
        {
            return await _context.Customerclass.Where(obj => obj.Customer == customer && obj.Type == "Selected").ToArrayAsync();

        }

        [Route("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Customerclass[]>> PurchasedCourses(string customer)
        {
            return await _context.Customerclass.Where(obj => obj.Customer == customer && obj.Type == "Purchased").ToArrayAsync();

        }

        [Route("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<bool>> CourseLimitReached(string customer)
        {
            var aplan = await _context.Customersubscription.SingleOrDefaultAsync(obj => obj.Customer == customer && obj.Status == "Active");
            if (aplan == null)
            {
                return BadRequest("Customer doesn't have an active plan");
            }
            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync(sp => sp.Subid == aplan.Subid);
            var usage = await _context.Subusage.SingleOrDefaultAsync(su => su.Subid == aplan.Id);
            if (usage == null)
            {
                return false;
            }
            if (usage.Coursetotal < asub.Coursecount)
            {
                return false;
            }
            return true;
        }

        [Route("[action]/{customer}/{coursecode}")]
        [HttpGet]
        public async Task<ActionResult<bool>> AlreadyEnrolled(string customer, string coursecode)
        {
            return await _context.Customerclass.SingleOrDefaultAsync(cc => cc.Customer == customer && cc.Coursecode == coursecode) != null;
        }

        [HttpPost]
        public async Task<ActionResult<Customerclass>> Post([FromBody] Customerclass obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }
            var cus = await _context.Customer.SingleOrDefaultAsync(c => c.Email == obj.Customer);
            var crs = await _context.Course.SingleOrDefaultAsync(c => c.Coursecode == obj.Coursecode);
            if (crs == null)
            {
                return BadRequest("Invalid course code");
            }
            if (crs.Status.ToLower() != "published")
            {
                return BadRequest("Customers can only enroll for published courses");
            }

            if (cus == null)
            {
                return BadRequest("Customer not found");
            }
            if (await _context.Customerclass.SingleOrDefaultAsync(ben => ben.Customer == obj.Customer && ben.Coursecode == obj.Coursecode) != null)
            {
                return BadRequest("This customer already enrolled for this course");
            }
            var target = await _context.Beneficiary.SingleOrDefaultAsync(ben => ben.Email == obj.Customer);
            if (target != null)
            {
                if (obj.Type.ToLower() == "purchased")
                {
                    return BadRequest("Beneficiary can't purchase course");
                }
                var aplan = await _context.Customersubscription.SingleOrDefaultAsync(cs => cs.Customer == target.Addedby && cs.Status == "Active");
                if (aplan == null)
                {
                    return BadRequest("Benefactor doesn't have an active business plan");
                }
                if (await _context.Customerclass.SingleOrDefaultAsync(cc => cc.Customer == target.Addedby && cc.Coursecode == obj.Coursecode) == null)
                {
                    return BadRequest("Beneficiary can only enroll for benefactor's course");
                }
                _context.Customerclass.Add(obj);
                await _context.SaveChangesAsync();
                email3 = "Your beneficiary " + cus.Lastname + " " + cus.Firstname + " just enrolled for the course: " + crs.Title + " [" + crs.Coursecode + "]";
                subject3 = "Staff Course Enrollment Notification";
                bccs3 = new string[] { target.Addedby };
                ThreadStart ts3 = new ThreadStart(dispatchMail3);
                Thread t3 = new Thread(ts3);
                t3.Start();
                return Created("api/Customerclass", obj);

            }
            else
            {

                if (obj.Type.ToLower() == "purchased")
                {
                    _context.Customerclass.Add(obj);
                    if (cus.Accounttype.ToLower() != "paid")
                    {
                        cus.Loyalitypoint += crs.Loyaltypoint;
                    }
                    await _context.SaveChangesAsync();
                    string[] aemails = _context.Users.Where(u => u.Role == "Admin").Select(u => u.Email).ToArray();
                    foreach (var e in aemails)
                    {
                        var note = new Notifications()
                        {
                            Type = "Purchase",
                            Target = e,
                            Remark = cus.Lastname + " " + cus.Firstname + " with email address " + cus.Email + " just purchased the course " + crs.Title + " [" + crs.Coursecode + "]"
                        };
                        _context.Notifications.Add(note);
                    }
                    await _context.SaveChangesAsync();
                    email = cus.Lastname + " " + cus.Firstname + " with email address " + cus.Email + " just purchased the course " + crs.Title + " [" + crs.Coursecode + "]";
                    subject = "Course Purchase Alert";
                    bccs = aemails;
                    ThreadStart ts = new ThreadStart(dispatchMail);
                    Thread t1 = new Thread(ts);
                    t1.Start();

                    string[] bemails = _context.Beneficiary.Where(b => b.Addedby == obj.Customer).Select(b => b.Email).ToArray();
                    foreach (var b in bemails)
                    {
                        var note = new Notifications()
                        {
                            Type = "NewCourse",
                            Target = b,
                            Remark = crs.Title + " [" + crs.Coursecode + "] has just been added to your organization's dashboard"
                        };
                        _context.Notifications.Add(note);
                    }
                    await _context.SaveChangesAsync();
                    email2 = crs.Title + " [" + crs.Coursecode + "] has just been added to your organization's dashboard";
                    subject2 = "New Course Notification";
                    bccs2 = bemails;
                    ThreadStart ts2 = new ThreadStart(dispatchMail2);
                    Thread t2 = new Thread(ts2);
                    t2.Start();

                    return Created("api/Customerclass", obj);
                }
                else
                {

                    if (crs.Freeonsubcription == 0)
                    {
                        return BadRequest("Premium course can only by purchased");
                    }
                    var aplan = await _context.Customersubscription.SingleOrDefaultAsync(cs => cs.Customer == obj.Customer && cs.Status == "Active");
                    if (aplan == null)
                    {
                        return BadRequest("Customer doesn't have an active plan");
                    }
                    var asub = await _context.Subscriptionplan.SingleOrDefaultAsync(sp => sp.Subid == aplan.Subid);
                    var usage = await _context.Subusage.SingleOrDefaultAsync(su => su.Subid == aplan.Id);
                    if (usage == null)
                    {
                        _context.Customerclass.Add(obj);
                        var lu = new Subusage()
                        {
                            Subid = aplan.Id,
                            Coursetotal = 1,
                            Classtotal = 0
                        };
                        _context.Subusage.Add(lu);
                        string[] bemails = _context.Beneficiary.Where(b => b.Addedby == obj.Customer).Select(b => b.Email).ToArray();
                        foreach (var b in bemails)
                        {
                            var note = new Notifications()
                            {
                                Type = "NewCourse",
                                Target = b,
                                Remark = crs.Title + " [" + crs.Coursecode + "] has just been added to your organization's dashboard"
                            };
                            _context.Notifications.Add(note);
                        }
                        await _context.SaveChangesAsync();
                        email2 = crs.Title + " [" + crs.Coursecode + "] has just been added to your organization's dashboard";
                        subject2 = "New Course Notification";
                        bccs2 = bemails;
                        ThreadStart ts2 = new ThreadStart(dispatchMail2);
                        Thread t2 = new Thread(ts2);
                        t2.Start();
                    }
                    else if (usage.Coursetotal < asub.Coursecount)
                    {
                        _context.Customerclass.Add(obj);
                        usage.Coursetotal += 1;
                        string[] bemails = _context.Beneficiary.Where(b => b.Addedby == obj.Customer).Select(b => b.Email).ToArray();
                        foreach (var b in bemails)
                        {
                            var note = new Notifications()
                            {
                                Type = "NewCourse",
                                Target = b,
                                Remark = crs.Title + " [" + crs.Coursecode + "] has just been added to your organization's dashboard"
                            };
                            _context.Notifications.Add(note);
                        }
                        await _context.SaveChangesAsync();
                        email2 = crs.Title + " [" + crs.Coursecode + "] has just been added to your organization's dashboard";
                        subject2 = "New Course Notification";
                        bccs2 = bemails;
                        ThreadStart ts2 = new ThreadStart(dispatchMail2);
                        Thread t2 = new Thread(ts2);
                        t2.Start();
                    }
                    else
                    {
                        return BadRequest("Course limit reached for the active plan");
                    }

                    await _context.SaveChangesAsync();
                    return Created("api/Customerclass", obj);
                }

            }

        }

        /* 
               [HttpPut ("{classid}")]
               public async Task<ActionResult> Put (int classid, [FromBody] Customerclass obj) {
                   var target = await _context.Customerclass.SingleOrDefaultAsync (nobj => nobj.Classid == classid);
                   if (target != null && ModelState.IsValid) {
                       _context.Entry (target).CurrentValues.SetValues (obj);
                       await _context.SaveChangesAsync ();
                       return Ok ();
                   }
                   return BadRequest ();
               }
              
                       [HttpDelete ("{classid}")]
                       public async Task<ActionResult> Delete (int classid) {
                           var target = await _context.Customerclass.SingleOrDefaultAsync (obj => obj.Classid == classid);
                           if (target != null) {
                               _context.Customerclass.Remove (target);
                               await _context.SaveChangesAsync ();
                               return Ok ();
                           }
                           return NotFound ();
                       } */
    }
}