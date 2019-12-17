using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class CustomersubscriptionController : ControllerBase {
        smelmsContext _context;
        public CustomersubscriptionController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Customersubscription[]>> Get () {
            return await _context.Customersubscription.ToArrayAsync ();
        }

        [HttpGet ("{id}")]
        public async Task<ActionResult<Customersubscription>> Get (int id) {
            var target = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Id == id);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Customersubscription>> GetActivePlan (string customer) {
            var target = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Status == "Active");
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<bool>> HasActivePlan (string customer) {
            return await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Status == "Active") != null;

        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<int>> GetCourseBalance (string customer) {
            var target = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Status == "Active");
            if (target == null) {
                return BadRequest ("Customer doesn't have an active plan");
            }
            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == target.Subid);
            var usage = await _context.Subusage.SingleOrDefaultAsync (su => su.Subid == target.Id);
            return asub.Coursecount - usage.Coursetotal;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<int>> GetClassBalance (string customer) {
            var target = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Status == "Active");
            if (target == null) {
                return BadRequest ("Customer doesn't have an active plan");
            }
            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == target.Subid);
            var usage = await _context.Subusage.SingleOrDefaultAsync (su => su.Subid == target.Id);
            return asub.Classcount - usage.Classtotal;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<bool>> HasPendingPlanChange (string customer) {
            var preq = await _context.Planchangerequest.SingleOrDefaultAsync (pr => pr.Customer == customer && pr.Status == "Pending");
            return preq != null;
        }

        [Route ("[action]")]
        [HttpPost]
        public async Task<ActionResult> Autorenew () {
            Customersubscription[] expsubs = await _context.Customersubscription.Where (sub => DateTime.Now.Date > sub.Expdate.Value.Date && sub.Status != "Expired").ToArrayAsync ();
            foreach (Customersubscription cs in expsubs) {
                var cus = await _context.Customer.SingleOrDefaultAsync (c => c.Email == cs.Customer);
                if (cs.Autorenew == 1) {
                    var preq = await _context.Planchangerequest.SingleOrDefaultAsync (pr => pr.Customer == cs.Customer && pr.Status == "Pending");
                    if (preq != null) { //has pending planchange request
                        Paymentlog pl = await _context.Paymentlog.SingleOrDefaultAsync (p => p.Refno == preq.Paymentref);
                        if (pl.Status.ToLower () == "accepted") { //pending planchange payment already recieved
                            var sub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == preq.Newplan);
                            var rnew = new Customersubscription () {
                                Customer = cs.Customer,
                                Paymentref = preq.Paymentref,
                                Subid = preq.Newplan
                            };
                            if (sub.Cycle.ToLower () == "monthly") {
                                rnew.Expdate = DateTime.Now.AddMonths (1);
                            } else if (sub.Cycle.ToLower () == "quarterly") {
                                rnew.Expdate = DateTime.Now.AddMonths (3);
                            } else {
                                rnew.Expdate = DateTime.Now.AddYears (1);
                            }
                            await _context.Customersubscription.AddAsync (rnew);
                            cs.Status = "Expired";
                            preq.Status = "Successful";
                            preq.Remark = "Plan change successful";
                            preq.Datechanged = DateTime.Now;
                            //Util.SendMail();//renewal successful
                        } else {
                            cs.Status = "Expired";
                            var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == cs.Customer && obj.Status == "Active");
                            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == aplan.Subid);
                            if ((cus.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                                cus.Accounttype = "Paid";
                            }
                            preq.Status = "Failed";
                            preq.Remark = "Plan change failed. Unsuccessful payment record found";
                            //Util.SendMail ();could not effect planchange, payment failed
                        }
                    } else { //no pending planchange request
                        var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == cs.Customer && obj.Status == "Active");
                        var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == aplan.Subid);
                        var ccard = await _context.Customercard.SingleOrDefaultAsync (cc => cc.Customer == cs.Customer);
                        if (ccard == null) { //no card found for customer
                            cs.Status = "Expired";
                            if ((cus.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                                cus.Accounttype = "Paid";
                            };

                            //Util.SendMail ();//renewal failed, not card found
                        } else { //customer has card, attempt payment
                            var plog = new Paymentlog (); // {
                            plog.Refno = Util.PesudoRefNo ();
                            plog.Customer = cs.Customer;
                            plog.Description = "Customer subscription renewal";
                            plog.Paymentmode = "Debit card";
                            plog.Cardnumber = ccard.Cardnumber;
                            plog.Status = "Accepted";
                            plog.Cashamount = asub.Amount;

                            // };
                            await _context.Paymentlog.AddAsync (plog);
                            await _context.SaveChangesAsync ();
                            if (plog.Status.ToLower () == "accepted") { //payment successful
                                var sub = await _context.Subscriptionplan.SingleOrDefaultAsync (sp => sp.Subid == cs.Subid);
                                var rnew = new Customersubscription () {
                                    Customer = cs.Customer,
                                    Paymentref = plog.Refno,
                                    Subid = cs.Subid
                                };
                                if (sub.Cycle.ToLower () == "monthly") {
                                    rnew.Expdate = DateTime.Now.AddMonths (1);
                                } else if (sub.Cycle.ToLower () == "quarterly") {
                                    rnew.Expdate = DateTime.Now.AddMonths (3);
                                } else {
                                    rnew.Expdate = DateTime.Now.AddYears (1);
                                }
                                await _context.Customersubscription.AddAsync (rnew);
                                cs.Status = "Expired";
                                //Util.SendMail();//renewal successful
                            } else {
                                cs.Status = "Expired";
                                if ((cus.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                                    cus.Accounttype = "Paid";
                                }

                                //Util.SendMail ();//renewal failed, payment declined
                            }
                        }
                    }
                } else { //autorenew off
                    cs.Status = "Expired";
                    var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == cs.Customer && obj.Status == "Active");
                    var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == aplan.Subid);
                    if ((cus.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                        cus.Accounttype = "Paid";
                    }
                    //Util.SendMail ();//renewal failed, sub expired
                }

            }
            await _context.SaveChangesAsync ();
            return Ok ();
        }

        [Route ("[action]")]
        [HttpPost]
        public async Task<ActionResult> ChangePlan ([FromBody] Planchangerequest obj) {
            var pl = await _context.Paymentlog.SingleOrDefaultAsync (p => p.Refno == obj.Paymentref);
            if (pl == null) {
                return BadRequest ("Invalid payment reference");
            }
            if (pl.Customer != obj.Customer) {
                return BadRequest ("Payment reference mismatch");
            }
            var target = await _context.Customersubscription.SingleOrDefaultAsync (cs => cs.Customer == obj.Customer && DateTime.Now.Date <= cs.Expdate.Value.Date && cs.Status != "Expired");
            if (target == null) {
                return BadRequest ("Customer doesn't have an active plan");
            }
            var preq = await _context.Planchangerequest.SingleOrDefaultAsync (pr => pl.Customer == obj.Customer && pr.Status == "Pending");
            if (preq != null) {
                return BadRequest ("This customer has a pending request");
            }
            var cus = await _context.Customer.SingleOrDefaultAsync (cc => cc.Email == obj.Customer);
            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == target.Subid);
            var nsub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == obj.Newplan);
            if ((cus.Accounttype.ToLower () != "paid" && target != null && asub.Type.ToLower () == "business") == true && nsub.Type.ToLower () == "individual") {
                return BadRequest ("Business owner can't change to inidividual plan");
            }
            int oldplan = target.Subid;
            switch (obj.Type.ToLower ()) {
                case "instantup":
                    target.Subid = obj.Newplan;
                    var sub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == obj.Newplan);
                    if (sub == null) {
                        return BadRequest ("Invalid subscription plan");
                    }
                    if (sub.Cycle.ToLower () == "monthly") {
                        target.Expdate = DateTime.Now.AddMonths (1);
                    } else if (sub.Cycle.ToLower () == "quarterly") {
                        target.Expdate = DateTime.Now.AddMonths (3);
                    } else {
                        target.Expdate = DateTime.Now.AddYears (1);
                    }
                    obj.Status = "Successful";

                    break;
                case "scheduledup":
                    obj.Status = "Pending";
                    break;
                case "down":
                    obj.Status = "Pending";
                    break;
                default:
                    return BadRequest ("Invalid request type");
            }
            obj.Oldplan = oldplan;
            await _context.Planchangerequest.AddAsync (obj);
            await _context.SaveChangesAsync ();
            return Ok ();
        }

        [Route ("[action]/{customer}")]
        [HttpPost]
        public async Task<ActionResult> ToogleAutorenew (string customer) {
            var target = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == customer && DateTime.Now.Date <= obj.Expdate.Value.Date && obj.Status != "Expired");
            if (target == null) {
                return BadRequest ("Customer doesn't have active plan");
            }
            if (target.Autorenew == 1) {
                target.Autorenew = 0;
            } else {
                target.Autorenew = 1;
            }
            await _context.SaveChangesAsync ();
            return Ok ();
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Customersubscription[]>> SubHistory (string customer) {
            return await _context.Customersubscription.Where (obj => obj.Customer == customer).ToArrayAsync ();
        }

        [Route ("[action]")]
        [HttpGet]
        public async Task<ActionResult<Customersubscription[]>> GetDue () {
            return await _context.Customersubscription.Where (sub => DateTime.Now.Date > sub.Expdate.Value.Date && sub.Status != "Expired").ToArrayAsync ();
        }

        [Route ("[action]/{status}")]
        [HttpGet]
        public async Task<ActionResult<Customersubscription[]>> GetByStatus (string status) {
            return await _context.Customersubscription.Where (sub => sub.Status == status).ToArrayAsync ();
        }
        /* [Route ("[action]")]
         [HttpPost]
         public async Task<ActionResult> InvalidateExpired () {
             Customersubscription[] expsubs = await _context.Customersubscription.Where (sub => DateTime.Now.Date > sub.Expdate.Value.Date && sub.Status != "Expired").ToArrayAsync ();
             foreach (Customersubscription cs in expsubs) {
                 cs.Status = "Expired";
                 var cus = await _context.Customer.SingleOrDefaultAsync (c => c.Email == cs.Customer);
                 cus.Accounttype = "Paid";
             }
             await _context.SaveChangesAsync ();
             return Ok ();
         }*/

        [HttpPost]
        public async Task<ActionResult<Customersubscription>> Post ([FromBody] Customersubscription obj) {
            if (!ModelState.IsValid) {
                return BadRequest ("Invalid model state");
            }

            if (await _context.Beneficiary.SingleOrDefaultAsync (ben => ben.Email == obj.Customer) != null) {
                return BadRequest ("Beneficiary can't subscribe");
            }
            var pay = await _context.Paymentlog.SingleOrDefaultAsync (pl => pl.Refno == obj.Paymentref);
            if (pay == null) {
                return BadRequest ("Invalid payment reference number");
            }
            if (pay.Customer != obj.Customer) {
                return BadRequest ("Payment reference mismatch");
            }

            var target = await _context.Customersubscription.SingleOrDefaultAsync (cs => cs.Customer == obj.Customer && DateTime.Now.Date <= cs.Expdate.Value.Date && cs.Status != "Expired");
            if (target != null) {
                return BadRequest ("Customer currently has an active plan");
            } else {
                var sub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == obj.Subid);
                if (sub == null) {
                    return BadRequest ("Invalid subscription plan");
                } else {
                    switch (sub.Cycle.ToLower ()) {
                        case "monthly":
                            obj.Expdate = obj.Subdate.Value.AddMonths (1);
                            break;
                        case "quarterly":
                            obj.Expdate = obj.Subdate.Value.AddMonths (3);
                            break;
                        case "annually":
                            obj.Expdate = obj.Subdate.Value.AddYears (1);
                            break;
                    }
                }

                _context.Customersubscription.Add (obj);
                await _context.SaveChangesAsync ();
                var cus = await _context.Customer.SingleOrDefaultAsync (c => c.Email == obj.Customer);
                if (cus != null) {
                    cus.Accounttype = "Subscribed";
                    await _context.SaveChangesAsync ();
                }
                return Created ("api/Customersubscription", obj);
            }
        }

        /*  [HttpPut ("{id}")]
         public async Task<ActionResult> Put (int id, [FromBody] Customersubscription obj) {
             var target = await _context.Customersubscription.SingleOrDefaultAsync (nobj => nobj.Id == id);
             if (target != null && ModelState.IsValid) {
                 _context.Entry (target).CurrentValues.SetValues (obj);
                 await _context.SaveChangesAsync ();
                 return Ok ();
             }
             return BadRequest ();
         }

         [HttpDelete ("{id}")]
         public async Task<ActionResult> Delete (int id) {
             var target = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Id == id);
             if (target != null) {
                 _context.Customersubscription.Remove (target);
                 await _context.SaveChangesAsync ();
                 return Ok ();
             }
             return NotFound ();
         }
         */
    }
}