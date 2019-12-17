using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class BeneficiaryController : ControllerBase {
        smelmsContext _context;
        public BeneficiaryController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Beneficiary[]>> Get () {
            return await _context.Beneficiary.ToArrayAsync ();
        }

        [HttpGet ("{email}")]
        public async Task<ActionResult<Beneficiary>> Get (string email) {
            var target = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }
        string email2, subject;
        string[] bccs;
        void dispatchMail () {
            Util.SendMail (email2, subject, bccs);
        }
        /*  [HttpPost]
         public async Task<ActionResult<Beneficiary>> Post ([FromBody] Beneficiary obj) {
             if (!ModelState.IsValid) {
                 return BadRequest ();
             } else {
                 _context.Beneficiary.Add (obj);
                 await _context.SaveChangesAsync ();
                 return Created ("api/Beneficiary", obj);
             }
         } */

        [HttpPut ("{email}")]
        public async Task<ActionResult> Put (string email, [FromBody] Beneficiary obj) {
            var target = await _context.Beneficiary.SingleOrDefaultAsync (nobj => nobj.Email == email);
            if (target != null && ModelState.IsValid) {
                _context.Entry (target).CurrentValues.SetValues (obj);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return BadRequest ();
        }

        [HttpDelete ("{email}")]
        public async Task<ActionResult> Delete (string email) {
            var target = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
            if (target != null) {
                _context.Beneficiary.Remove (target);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return NotFound ();
        }

        [Route ("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> InviteLimitReached (string email) {
            var benefactor = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == email);
            if (benefactor == null || benefactor.Accounttype.ToLower () != "subscribed") {
                return BadRequest ("Benefactor doesn't exist");
            }
            var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == email && obj.Status == "Active");
            var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == aplan.Subid);
            if ((benefactor.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                return BadRequest ("Benefactor dosen't have an active business subscription");
            }

            int bcount = await _context.Beneficiary.CountAsync (b => b.Addedby == email);
            return bcount == asub.Beneficiarycount;

        }

        [Route ("[action]/{email}/{isprivileged}/{addedby}")]
        [HttpPost]
        public async Task<ActionResult> Invite (string email, byte isprivileged, string addedby) {
            try {
                if (!new EmailAddressAttribute ().IsValid (email)) {
                    return BadRequest ("Invalid email");
                }
                var benefactor = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == addedby);
                if (benefactor == null || benefactor.Accounttype.ToLower () != "subscribed") {
                    return BadRequest ("Benefactor doesn't exist");
                }
                var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == addedby && obj.Status == "Active");
                var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == aplan.Subid);
                if ((benefactor.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                    return BadRequest ("Benefactor dosen't have an active business subscription");
                }

                int bcount = await _context.Beneficiary.CountAsync (b => b.Addedby == addedby);
                if (bcount == asub.Beneficiarycount) {
                    return BadRequest ("Beneficiary limit reached");
                }
                var target = await _context.Users.SingleOrDefaultAsync (obj => obj.Email == email);
                var target2 = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
                var target3 = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == email);
                if (target != null || target2 != null || target3 != null) {
                    return BadRequest ("User already exist");
                }

                Util.SendMail ("Hi,<br>You have been invited to " + benefactor.Companyname + "'s  Learning Management System Workspace . Kindly click <a href='https://www.smelms.com/staff/signup?email=" + email + "'>here</a> to complete your signup.", "SME UPTURN LMS INVITE", email);
                Customer cus = new Customer {
                    Loyalitypoint = 0,
                    Email = email,
                    Isverified = 1,
                    Accounttype = "subscribed",
                    Companyname = benefactor.Companyname
                };
                _context.Customer.Add (cus);
                await _context.SaveChangesAsync ();
                Beneficiary ben = new Beneficiary {
                    Email = email,
                    Addedby = addedby,
                    Status = "Active",
                    Ispriviledge = isprivileged
                };
                _context.Beneficiary.Add (ben);
                await _context.SaveChangesAsync ();
                return Ok ();
            } catch (Exception ex) {
                return StatusCode (500, "Internal server error: " + ex.Message);
            }

        }

        [Route ("[action]/{email}/{firstname}/{lastname}/{isprivileged}/{addedby}")]
        [HttpPost]
        public async Task<ActionResult> InviteWithName (string email, string firstname, string lastname, byte isprivileged, string addedby) {
            try {
                if (!new EmailAddressAttribute ().IsValid (email)) {
                    return BadRequest ("Invalid email");
                }
                var benefactor = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == addedby);
                if (benefactor == null || benefactor.Accounttype.ToLower () != "subscribed") {
                    return BadRequest ("Benefactor doesn't exist");
                }
                var aplan = await _context.Customersubscription.SingleOrDefaultAsync (obj => obj.Customer == addedby && obj.Status == "Active");
                var asub = await _context.Subscriptionplan.SingleOrDefaultAsync (s => s.Subid == aplan.Subid);
                if ((benefactor.Accounttype.ToLower () != "paid" && aplan != null && asub.Type.ToLower () == "business") == false) {
                    return BadRequest ("Benefactor dosen't have an active business subscription");
                }
                int bcount = await _context.Beneficiary.CountAsync (b => b.Addedby == addedby);
                if (bcount == asub.Beneficiarycount) {
                    return BadRequest ("Beneficiary limit reached");
                }
                var target = await _context.Users.SingleOrDefaultAsync (obj => obj.Email == email);
                var target2 = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
                var target3 = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == email);
                if (target != null || target2 != null || target3 != null) {
                    return BadRequest ("User already exist");
                }

                Util.SendMail ("Hi " + firstname + ",<br>You have been invited to " + benefactor.Companyname + "'s  Learning Management System Workspace . Kindly click <a href='https://www.smelms.com/staff/signup?email=" + email + "&fname=" + firstname + "&lname=" + lastname + "'>here</a> to complete your signup.", "SME UPTURN LMS INVITE", email);
                Customer cus = new Customer {
                    Firstname = firstname,
                    Lastname = lastname,
                    Loyalitypoint = 0,
                    Email = email,
                    Isverified = 1,
                    Accounttype = "subscribed",
                    Companyname = benefactor.Companyname
                };
                _context.Customer.Add (cus);
                await _context.SaveChangesAsync ();
                Beneficiary ben = new Beneficiary {
                    Email = email,
                    Addedby = addedby,
                    Status = "Active",
                    Ispriviledge = isprivileged
                };
                _context.Beneficiary.Add (ben);
                await _context.SaveChangesAsync ();
                return Ok ();
            } catch (Exception ex) {
                return StatusCode (500, "Internal server error: " + ex.ToString ());
            }

        }

        [Route ("[action]/{email}/{firstname}/{lastname}/{password}")]
        [HttpPost]
        public async Task<ActionResult> Signup (string email, string firstname, string lastname, string password) {
            var target = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == email);
            var target2 = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
            if (target == null || target2 == null) {
                return BadRequest ("User not found");
            }
            target.Firstname = firstname;
            target.Lastname = lastname;
            target.Password = Util.Encrypt (password);
            await _context.SaveChangesAsync ();
            email2 = "Your staff " + target.Lastname + " " + target.Firstname + " just accepted the LMS invite.";
            subject = "Invite Acceptance Alert";
            bccs = new string[] { target2.Addedby };
            ThreadStart ts = new ThreadStart (dispatchMail);
            Thread t1 = new Thread (ts);
            t1.Start ();
            return Ok ();
        }

        [Route ("[action]/{email}")]
        [HttpPost]
        public async Task<ActionResult> UploadDp (string email) {
            var target = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);

            if (target == null || Request.Form.Files.Count == 0) {
                return BadRequest ("User not found or request file missing");
            }
            try {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine ("Res", "Dps");
                var pathToSave = Path.Combine (Directory.GetCurrentDirectory (), folderName);

                if (file.Length > 0) {
                    var fullPath = Path.Combine (pathToSave, email.ToLower () + ".png");
                    using (var stream = new FileStream (fullPath, FileMode.Create)) {
                        await file.CopyToAsync (stream);
                    }

                    return Ok ();
                } else {
                    return BadRequest ("Empty file");
                }
            } catch (InvalidOperationException ex) {
                return BadRequest (ex.Message);
            } catch (Exception ex) {
                return StatusCode (500, "Internal server error: " + ex.Message);
            }
        }

        [Route ("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetDp (string email) {
            var target = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
            if (target == null) {
                return NotFound ("User not found");
            }
            var folderName = Path.Combine ("Res", "Dps");
            var pathToSave = Path.Combine (Directory.GetCurrentDirectory (), folderName);
            var fullPath = Path.Combine (pathToSave, email.ToLower () + ".png");
            if (!System.IO.File.Exists (fullPath)) {
                return NotFound ();
            }
            return Path.Combine (folderName, email.ToLower () + ".png");;
        }

        [Route ("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetDpRaw (string email) {
            var target = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
            if (target == null) {
                return NotFound ();
            }
            var folderName = Path.Combine ("Res", "Dps");
            var pathToSave = Path.Combine (Directory.GetCurrentDirectory (), folderName);
            var fullPath = Path.Combine (pathToSave, email.ToLower () + ".png");
            if (!System.IO.File.Exists (fullPath)) {
                return NotFound ();
            }
            return await System.IO.File.ReadAllBytesAsync (Path.Combine (folderName, email.ToLower () + ".png").ToString ());
        }

        [Route ("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> Exists (string email) {
            var target = await _context.Users.SingleOrDefaultAsync (obj => obj.Email == email);
            var target2 = await _context.Beneficiary.SingleOrDefaultAsync (obj => obj.Email == email);
            var target3 = await _context.Customer.SingleOrDefaultAsync (obj => obj.Email == email);
            return target != null || target2 != null || target3 != null;
        }

        [Route ("[action]/{owner}")]
        [HttpGet]
        public async Task<ActionResult<Beneficiary[]>> OwnerBeneficiaries (string owner) {
            return await _context.Beneficiary.Where (b => b.Addedby == owner).ToArrayAsync ();
        }

        [Route ("[action]/{owner}")]
        [HttpGet]
        public async Task<ActionResult<int>> OwnerBeneficiariesCount (string owner) {
            return await _context.Beneficiary.CountAsync (b => b.Addedby == owner);
        }

    }
}