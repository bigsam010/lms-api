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
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        smelmsContext _context;
        public UsersController(smelmsContext _context)
        {
            this._context = _context;
        }
        string email2, subject;
        string[] bccs;
        void dispatchMail()
        {
            Util.SendMail(email2, subject, bccs);
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {

                return Ok(await _context.Users.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }

        [Route("[action]/{role}")]
        [HttpGet]
        public async Task<ActionResult> GetAllByRole(string role)
        {
            try
            {
                List<string> roles = new List<string>()
                {
                    "author","admin"
                };
                if (!role.Contains(role.ToLower()))
                {
                    return BadRequest("Invalid role");
                }

                return Ok(await _context.Users.Where(rl => rl.Role == role).ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }
        [HttpGet("{email}")]
        public async Task<ActionResult<Users>> Get(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }
        private class AdvUser : Users
        {
            public DateTime lastlogin { set; get; }
            public byte[] dp { set; get; }
        }
        [Route("[action]/{role}")]
        [HttpGet]
        public async Task<ActionResult> GetByRoleSortByAge(string role, string mode = "desc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            try
            {

                if (role.ToLower() != "admin" && role.ToLower() != "author")
                {
                    return BadRequest("Invalid user role");
                }
                int skip = (pageNo - 1) * pageSize;
                long total = _context.Users.Where(obj => obj.Role == role).Count();
                var urs = await _context.Users.Where(obj => obj.Role == role).OrderByDescending(u => u.Dateadded).Skip(skip).Take(pageSize).ToListAsync();
                if (mode != "desc")
                {
                    urs = await _context.Users.Where(obj => obj.Role == role).OrderBy(u => u.Dateadded).Skip(skip).Take(pageSize).ToListAsync();
                }
                return Ok(new PagedResult<Users>(urs, pageNo, pageSize, total));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }

        [Route("[action]/{role}")]
        [HttpGet]
        public async Task<ActionResult> GetByRoleSortByName(string mode="asc",string role="asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            try
            {


                if (role.ToLower() != "admin" && role.ToLower() != "author")
                {
                    return BadRequest("Invalid user role");
                }
                int skip = (pageNo - 1) * pageSize;
                long total = _context.Users.Where(obj => obj.Role == role).Count();
                var urs = await _context.Users.Where(obj => obj.Role == role).OrderBy(u => u.Firstname).Skip(skip).Take(pageSize).ToListAsync();
                if (mode != "asc") {
                    urs = await _context.Users.Where(obj => obj.Role == role).OrderByDescending(u => u.Firstname).Skip(skip).Take(pageSize).ToListAsync();
                }
                return Ok(new PagedResult<Users>(urs, pageNo, pageSize, total));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }
        [Route("[action]/{role}")]
        [HttpGet]
        public async Task<ActionResult> GetByRole(string role, int pageNo = 1, int pageSize = 10)
        {
            try
            {

                if (role.ToLower() != "admin" && role.ToLower() != "author")
                {
                    return BadRequest("Invalid user role");
                }
                int skip = (pageNo - 1) * pageSize;
                long total = _context.Users.Where(obj => obj.Role == role).Count();
                var urs = await _context.Users.Where(obj => obj.Role == role).Skip(skip).Take(pageSize).OrderBy(u => u.Firstname).ToListAsync();
                return Ok(new PagedResult<Users>(urs, pageNo, pageSize, total));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }

        [Route("[action]/{email}/{isprivileged}")]
        [HttpPost]
        public async Task<ActionResult> InviteAuthor(string email, byte isprivileged)
        {
            try
            {
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest("Invalid email");
                }
                var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
                var target2 = await _context.Beneficiary.SingleOrDefaultAsync(obj => obj.Email == email);
                var target3 = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
                if (target != null || target2 != null || target3 != null)
                {
                    return BadRequest("User already exist");
                }
                Util.SendMail("Hi,<br>You have been invited to Sme Upturn Learning Management System. Kindly click <a href='https://upturn-lms.netlify.com/auth/register?email=" + email + "'>here</a> to complete your signup.", "SME UPTURN LMS INVITE", email);
                Users usr = new Users
                {
                    Email = email,
                    Role = "Author",
                    Isprivileged = isprivileged
                };
                _context.Users.Add(usr);
                await _context.SaveChangesAsync();
                return Created("api/Users", usr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        [Route("[action]/{email}/{firstname}/{lastname}/{isprivileged}")]
        [HttpPost]
        public async Task<ActionResult> InviteAuthorWithName(string email, string firstname, string lastname, byte isprivileged)
        {
            try
            {
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest("Invalid email");
                }
                var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
                var target2 = await _context.Beneficiary.SingleOrDefaultAsync(obj => obj.Email == email);
                var target3 = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
                if (target != null || target2 != null || target3 != null)
                {
                    return BadRequest("User already exist");
                }
                Util.SendMail("Hi " + firstname + ",<br>You have been invited to Sme Upturn Learning Management System. Kindly click <a href='https://upturn-lms.netlify.com/auth/register?email=" + email + "&fname=" + firstname + "&lname=" + lastname + "'>here</a> to complete your signup.", "SME UPTURN LMS INVITE", email);
                Users usr = new Users
                {
                    Email = email,
                    Role = "Author",
                    Isprivileged = isprivileged,
                    Firstname = firstname,
                    Lastname = lastname
                };
                _context.Users.Add(usr);
                await _context.SaveChangesAsync();
                return Created("api/Users", usr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.ToString());
            }
        }

        [Route("[action]/{email}")]
        [HttpPost]
        public async Task<ActionResult> InviteAdmin(string email)
        {
            try
            {
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest("Invalid email");
                }
                var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
                var target2 = await _context.Beneficiary.SingleOrDefaultAsync(obj => obj.Email == email);
                var target3 = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
                if (target != null || target2 != null || target3 != null)
                {
                    return BadRequest("User already exist");
                }
                Util.SendMail("Hi,<br>You have been invited to Sme Upturn Learning Management System. Kindly click <a href='https://upturn-lms.netlify.com/auth/register?email=" + email + "'>here</a> to complete your signup.", "SME UPTURN LMS INVITE", email);
                Users usr = new Users
                {
                    Email = email,
                    Role = "Admin",
                    Isprivileged = 1
                };
                _context.Users.Add(usr);
                await _context.SaveChangesAsync();
                return Created("api/Users", usr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        [Route("[action]/{email}/{firstname}/{lastname}")]
        [HttpPost]
        public async Task<ActionResult> InviteAdminWithName(string email, string firstname, string lastname)
        {
            try
            {
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest("Invalid email");
                }
                var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
                var target2 = await _context.Beneficiary.SingleOrDefaultAsync(obj => obj.Email == email);
                var target3 = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
                if (target != null || target2 != null || target3 != null)
                {
                    return BadRequest("User already exist");
                }
                Util.SendMail("Hi " + firstname + ",<br>You have been invited to Sme Upturn Learning Management System. Kindly click <a href='https://upturn-lms.netlify.com/auth/register?email=" + email + "&fname=" + firstname + "&lname=" + lastname + "'>here</a> to complete your signup.", "SME UPTURN LMS INVITE", email);
                Users usr = new Users
                {
                    Email = email,
                    Role = "Admin",
                    Isprivileged = 1,
                    Firstname = firstname,
                    Lastname = lastname
                };
                _context.Users.Add(usr);
                await _context.SaveChangesAsync();
                return Created("api/Users", usr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{email}")]
        public async Task<ActionResult> Put(string email, [FromBody] Users obj)
        {
            var target = await _context.Users.SingleOrDefaultAsync(nobj => nobj.Email == email);
            if (target != null && ModelState.IsValid)
            {
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("User not found");
        }

        [HttpDelete("{email}")]
        public async Task<ActionResult> Delete(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target != null)
            {
                target.Status = "Blocked";
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }


        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult> SearchByName(string name)
        {
            return Ok(await _context.Users.Where(u => u.Firstname.Contains(name) || u.Lastname.Contains(name)).ToListAsync());
        }

        [Route("[action]/{email}/{firstname}/{lastname}/{password}")]
        [HttpPost]
        public async Task<ActionResult> Signup(string email, string firstname, string lastname, string password)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return BadRequest("User does not exist");
            }
            if (target.Password != null)
            {
                return BadRequest("User already completed signup");
            }
            target.Firstname = firstname;
            target.Lastname = lastname;
            target.Password = Util.Encrypt(password);
            await _context.SaveChangesAsync();
            string[] aemails = _context.Users.Where(u => u.Role == "Admin").Select(u => u.Email).ToArray();
            email2 = target.Lastname + " " + target.Firstname + " just accepted the LMS invite.";
            subject = "Invite Acceptance Alert";
            bccs = aemails;
            ThreadStart ts = new ThreadStart(dispatchMail);
            Thread t1 = new Thread(ts);
            t1.Start();
            return Ok(target);
        }

        [Route("[action]/{email}/{password}")]
        [HttpGet]
        public async Task<ActionResult<bool>> Authenticate(string email, string password)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email && obj.Status != "Blocked");
            if (target == null)
            {
                return false;
            }
            var res = target.Password == Util.Encrypt(password);
            if (res)
            {
                var le = new Loginentry
                {
                    Client = email
                };
                await _context.AddAsync(le);
                target.Lastlogin = le.Logindate;
                await _context.SaveChangesAsync();
            }
            return res;

        }

        [Route("[action]/{email}")]
        [HttpPost]
        public async Task<ActionResult> Unblock(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target != null)
            {
                target.Status = "Active";
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> Isblocked(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target != null)
            {
                return target.Status == "Blocked";
            }
            return NotFound();
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> Exists(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            var target2 = await _context.Beneficiary.SingleOrDefaultAsync(obj => obj.Email == email);
            var target3 = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            return target != null || target2 != null || target3 != null;
        }

        [Route("[action]/{email}/{oldpassword}/{newpassword}")]
        [HttpPost]
        public async Task<ActionResult> ChangePassword(string email, string oldpassword, string newpassword)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email && obj.Status != "Blocked");
            if (target != null)
            {
                if (target.Password == Util.Encrypt(oldpassword))
                {
                    target.Password = Util.Encrypt(newpassword);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return BadRequest("Invalid password");
                }
            }
            return NotFound();
        }

        [Route("[action]/{email}")]
        [HttpPost]
        public async Task<ActionResult> SendResetLink(string email)
        {
            try
            {
                if (!new EmailAddressAttribute().IsValid(email))
                {
                    return BadRequest("Invalid email");
                }
                var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email.ToLower() == email.ToLower());
                if (target == null)
                {
                    return NotFound("User not found");
                }
                string tk = Util.GenToken();
                Util.SendMail("Click <a href='https://upturn-lms.netlify.com/auth/reset-password?email=" + email + "&token=" + tk + "'>here</a> to reset your password.", "Password Reset", email);
                var rreq = new Passwordreset
                {
                    Customer = email,
                    Token = tk,
                    Expdate = DateTime.Now.AddHours(1)

                };
                _context.Passwordreset.Add(rreq);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        [Route("[action]/{email}/{newpassword}")]
        [HttpPost]
        public async Task<ActionResult> ResetPassword(string email, string token, string newpassword)
        {
            var target = await _context.Passwordreset.SingleOrDefaultAsync(obj => obj.Customer == email && obj.Token.ToString() == token && DateTime.Now <= obj.Expdate);
            if (target != null)
            {
                var user = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email && obj.Status != "Blocked");
                user.Password = Util.Encrypt(newpassword);
                _context.Passwordreset.Remove(target);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Invalid or expired token");
        }

        [Route("[action]/{email}")]
        [HttpPost]
        public async Task<ActionResult> UploadDp(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null || Request.Form.Files.Count == 0)
            {
                return BadRequest("User not found or request file missing");
            }
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "Dps");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, email.ToLower() + ".png");
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok(Path.Combine(folderName, email.ToLower() + ".png"));
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

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetDp(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            var folderName = Path.Combine("Res", "Dps");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, email.ToLower() + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return Path.Combine(folderName, email.ToLower() + ".png");
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetDpRaw(string email)
        {
            var target = await _context.Users.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            var folderName = Path.Combine("Res", "Dps");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, email.ToLower() + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return await System.IO.File.ReadAllBytesAsync(Path.Combine(folderName, email.ToLower() + ".png").ToString());
        }

    }

}