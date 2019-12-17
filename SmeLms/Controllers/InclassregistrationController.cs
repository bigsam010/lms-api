using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class InclassregistrationController : ControllerBase {
        smelmsContext _context;
        public InclassregistrationController (smelmsContext _context) {
            this._context = _context;
        }
        string email, subject;
        string[] bccs;

        string email2, subject2;
        string[] bccs2;
        void dispatchMail () {
            Util.SendMail (email, subject, bccs);
        }

        void dispatchMail2 () {
            Util.SendMail (email2, subject2, bccs2);
        }

        [HttpGet]
        public async Task<ActionResult<Inclassregistration[]>> Get () {
            return await _context.Inclassregistration.ToArrayAsync ();
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Inclassregistration[]>> MyClasses (string customer) {
            return await _context.Inclassregistration.Where (ic => ic.Email == customer).ToArrayAsync ();
        }

        [Route ("[action]/{class}")]
        [HttpGet]
        public async Task<ActionResult<Inclassregistration[]>> Attendees (int Class) {
            return await _context.Inclassregistration.Where (ic => ic.Classid == Class).ToArrayAsync ();
        }

        [Route ("[action]/{customer}/{class}")]
        [HttpGet]
        public async Task<ActionResult<Inclassregistration[]>> Invitees (string customer, int Class) {
            return await _context.Inclassregistration.Where (ic => ic.Classid == Class && ic.Invitedby == customer).ToArrayAsync ();
        }

        [Route ("[action]/{customer}/{class}")]
        [HttpGet]
        public ActionResult<bool> HasInvitees (string customer, int Class) {
            return _context.Inclassregistration.Count (ic => ic.Classid == Class && ic.Invitedby == customer) > 0;
        }

        private string GenRegId () {
            string id = "CA";
            do {
                for (int i = 1; i <= 8; i++) {
                    id += new Random ().Next (0, 9);
                }
            }
            while (_context.Inclassregistration.SingleOrDefault (ic => ic.Regid == id) != null);
            return id;
        }

        [HttpGet ("{regid}")]
        public async Task<ActionResult<Inclassregistration>> Get (string regid) {
            var target = await _context.Inclassregistration.SingleOrDefaultAsync (obj => obj.Regid == regid);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<bool>> ClassLimitReached (string customer) {
            var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Status == "Active");
            if (aplan == null) {
                return BadRequest ("Customer doesn't have an active plan");
            }
            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == aplan.Subid);
            var usage = await _context.Subusage.SingleOrDefaultAsync (su => su.Subid == aplan.Id);
            if (usage == null) {
                return false;
            }
            if (usage.Classtotal < asub.Classcount) {
                return false;
            }
            return true;
        }

        [Route ("[action]/{customer}/{class}")]
        [HttpGet]
        public async Task<ActionResult<bool>> AlreadyEnrolled (string customer, int Class) {
            return await _context.Inclassregistration.SingleOrDefaultAsync (ic => ic.Email == customer && ic.Classid == Class) != null;
        }

        [HttpPost]
        public async Task<ActionResult<Inclassregistration>> Post ([FromBody] Inclassregistration obj) {
            if (!ModelState.IsValid) {
                return BadRequest ("Invalid model state");
            }
            var cus = await _context.Customer.SingleOrDefaultAsync (c => c.Email == obj.Email);
            var iclass = await _context.Inclass.SingleOrDefaultAsync (ic => ic.Classid == obj.Classid);
            if (iclass == null) {
                return BadRequest ("Invalid class");
            }
            if (await _context.Inclassregistration.SingleOrDefaultAsync (ic => ic.Email == obj.Email && ic.Classid == obj.Classid) != null) {
                return BadRequest ("Attendee already registered for this class");
            }
            obj.Regid = GenRegId ();
            var isBen = await _context.Beneficiary.SingleOrDefaultAsync (ben => ben.Email == obj.Email) != null;
            if (cus != null && !isBen) { //if attendee is a registered customer
                obj.Fullname = cus.Lastname + " " + cus.Firstname;
                if (obj.Type.ToLower () == "purchased") {
                    _context.Inclassregistration.Add (obj);
                    //cus.Loyalitypoint += iclass.Loyalitypoint;
                    string[] aemails = _context.Users.Where (u => u.Role == "Admin").Select (u => u.Email).ToArray ();

                    foreach (var e in aemails) {
                        var note = new Notifications () {
                            Type = "Purchase",
                            Target = e,
                            Remark = cus.Lastname + " " + cus.Firstname + " with email address " + cus.Email + " just purchased a slot in the " + iclass.Title
                        };
                        _context.Notifications.Add (note);
                    }
                    await _context.SaveChangesAsync ();
                    email = cus.Lastname + " " + cus.Firstname + " with email address " + cus.Email + " just purchased a slot in the " + iclass.Title;
                    subject = "Class Purchase Alert";
                    bccs = aemails;
                    ThreadStart ts = new ThreadStart (dispatchMail);
                    Thread t1 = new Thread (ts);
                    t1.Start ();

                } else {
                    var aplan = await _context.Customersubscription.SingleOrDefaultAsync (cs => cs.Customer == obj.Email && cs.Status == "Active");
                    if (aplan == null) {
                        return BadRequest ("Customer doesn't have an active plan");
                    }
                    var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == aplan.Subid);
                    var usage = await _context.Subusage.SingleOrDefaultAsync (su => su.Subid == aplan.Id);
                    if (usage == null) {
                        var su = new Subusage () {
                        Subid = aplan.Id,
                        Coursetotal = 0,
                        Classtotal = 1
                        };
                        _context.Inclassregistration.Add (obj);
                        _context.Subusage.Add (su);

                    } else if (usage.Classtotal < asub.Classcount) {
                        _context.Inclassregistration.Add (obj);
                        usage.Classtotal++;

                    } else {
                        return BadRequest ("Class slot limit reached");
                    }
                }
            } else {
                var iby = await _context.Customer.SingleOrDefaultAsync (ib => ib.Email == obj.Invitedby);
                if (iby == null) {
                    return BadRequest ("Attendee can only be inivited by registered customers");
                }
                var aplan = await _context.Customersubscription.SingleOrDefaultAsync (cs => cs.Customer == obj.Invitedby && cs.Status == "Active");
                if (aplan == null) {
                    return BadRequest ("This Customer doesn't have an active plan");
                }
                if (isBen) {
                    obj.Fullname = cus.Lastname + " " + cus.Firstname;
                    var not = new Notifications () {
                        Type = "Invite",
                        Target = obj.Email,
                        Remark = "A slot has been reserved for you in the class: " + iclass.Title + ". Starting on " + iclass.Startdate.ToLongDateString ()
                    };
                    _context.Notifications.Add (not);
                    email2 = "A slot has been reserved for you in the class: " + iclass.Title + ". Starting on " + iclass.Startdate.ToLongDateString ();
                    subject2 = "Class Slot Notification";
                    bccs2 = new String[] { obj.Email };
                    ThreadStart ts2 = new ThreadStart (dispatchMail2);
                    Thread t2 = new Thread (ts2);
                    t2.Start ();
                }
                var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == aplan.Subid);
                var usage = await _context.Subusage.SingleOrDefaultAsync (su => su.Subid == aplan.Id);
                if (usage == null) {
                    var su = new Subusage () {
                    Subid = aplan.Id,
                    Coursetotal = 0,
                    Classtotal = 1
                    };
                    _context.Inclassregistration.Add (obj);
                    _context.Subusage.Add (su);
                } else if (usage.Classtotal < asub.Classcount) {
                    _context.Inclassregistration.Add (obj);
                    usage.Classtotal++;
                } else {
                    return BadRequest ("Class slot limit reached");
                }
            }
            await _context.SaveChangesAsync ();
            return Created ("api/Inclassregistration", obj);

        }

        /*  [HttpPut ("{regid}")]
         public async Task<ActionResult> Put (string regid, [FromBody] Inclassregistration obj) {
             var target = await _context.Inclassregistration.SingleOrDefaultAsync (nobj => nobj.Regid == regid);
             if (target != null && ModelState.IsValid) {
                 _context.Entry (target).CurrentValues.SetValues (obj);
                 await _context.SaveChangesAsync ();
                 return Ok ();
             }
             return BadRequest ();
         }

         [HttpDelete ("{regid}")]
         public async Task<ActionResult> Delete (string regid) {
             var target = await _context.Inclassregistration.SingleOrDefaultAsync (obj => obj.Regid == regid);
             if (target != null) {
                 _context.Inclassregistration.Remove (target);
                 await _context.SaveChangesAsync ();
                 return Ok ();
             }
             return NotFound ();
         } */
    }
}