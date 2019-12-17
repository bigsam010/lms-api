using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        smelmsContext _context;
        public CustomerController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult> Get(string mode="asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            var benes = await _context.Beneficiary.Select(b => b.Email).ToListAsync();
            int skip = (pageNo - 1) * pageSize;
            var customers = await _context.Customer.Where(c => !benes.Contains(c.Email)).ToListAsync();
            int total = customers.Count();
            var records = customers.OrderBy(c => c.Firstname).Skip(skip).Take(pageSize).ToList();
            if (mode != "asc") {
                records = customers.OrderByDescending(c => c.Firstname).Skip(skip).Take(pageSize).ToList();
            }
            return Ok(new PagedResult<Customer>(records, pageNo, pageSize, total));
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortByAge(string mode = "desc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            var benes = await _context.Beneficiary.Select(b => b.Email).ToListAsync();
            int skip = (pageNo - 1) * pageSize;
            var customers = await _context.Customer.Where(c => !benes.Contains(c.Email)).ToListAsync();
            int total = customers.Count();
            var records = customers.OrderByDescending(c => c.Joindate).Skip(skip).Take(pageSize).ToList();
            if (mode != "desc")
            {
                records = customers.OrderBy(c => c.Joindate).Skip(skip).Take(pageSize).ToList();
            }
            return Ok(new PagedResult<Customer>(records, pageNo, pageSize, total));
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var benes = await _context.Beneficiary.Select(b => b.Email).ToListAsync();
            var customers = await _context.Customer.Where(c => !benes.Contains(c.Email)).ToListAsync();

            return Ok(customers);
        }
        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult> Search(string name)
        {
            return Ok(await _context.Customer.Where(c => c.Firstname.Contains(name) || c.Lastname.Contains(name)).ToListAsync());
        }
        [HttpGet("{email}")]
        public async Task<ActionResult<Customer>> Get(string email)
        {

            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> Post([FromBody] Customer obj)
        {
            if (!new EmailAddressAttribute().IsValid(obj.Email))
            {
                return BadRequest("Invalid email");
            }
            var target = await _context.Users.SingleOrDefaultAsync(o => o.Email == obj.Email);
            var target2 = await _context.Beneficiary.SingleOrDefaultAsync(o => o.Email == obj.Email);
            var target3 = await _context.Customer.SingleOrDefaultAsync(o => o.Email == obj.Email);
            if (target != null || target2 != null || target3 != null)
            {
                return BadRequest("User already exist");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            obj.Password = Util.Encrypt(obj.Password);
            obj.Isverified = 0;
            if (obj.Accounttype.ToLower() == "paid")
            {
                obj.Companyname = obj.Firstname;
            }
            string tk = Util.GenToken();
            obj.Verificationtoken = tk;
            Util.SendMail("Hi " + obj.Firstname + ",<br>You are welcome on board. Kindly click <a href='https://www.smelms.com/customer/verify?email=" + obj.Email + "&token=" + tk + "'>here</a> to verify your email.", "SME UPTURN LMS", obj.Email);
            _context.Customer.Add(obj);
            await _context.SaveChangesAsync();
            return Created("api/Customer", obj);

        }

        [Route("[action]/{customer}/{point}")]
        [HttpPost]
        public async Task<ActionResult> GiftLoyaltypoint(string customer, int point)
        {
            var cus = await _context.Customer.SingleOrDefaultAsync(c => c.Email == customer);
            if (cus != null)
            {
                cus.Loyalitypoint += point;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("User not found");
        }

        [Route("[action]/{email}/{password}")]
        [HttpGet]
        public async Task<ActionResult<bool>> Authenticate(string email, string password)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email && obj.Status != "Blocked");
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
                await _context.SaveChangesAsync();
            }
            return res;

        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> Isverified(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return false;
            }
            return target.Isverified == 1;

        }

        [Route("[action]/{email}/{token}")]
        [HttpPost]
        public async Task<ActionResult> Verify(string email, string token)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            if (target.Verificationtoken != token)
            {
                return BadRequest("Invalid token");
            }
            target.Isverified = 1;
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpPut("{email}")]
        public async Task<ActionResult> Put(string email, [FromBody] Customer obj)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(nobj => nobj.Email == email);
            if (target != null && ModelState.IsValid)
            {
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{email}")]
        public async Task<ActionResult> Delete(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);

            if (target != null)
            {
                target.Status = "Blocked";
                await _context.SaveChangesAsync();
                return Ok();
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

        [Route("[action]/{email}")]
        [HttpPost]
        public async Task<ActionResult> Unblock(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
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
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target != null)
            {
                return target.Status == "Blocked";
            }
            return NotFound();
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsBeneficiary(string email)
        {
            var target = await _context.Beneficiary.SingleOrDefaultAsync(obj => obj.Email == email);
            return target != null;
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsBusinessOwner(string email)
        {
            var cus = await _context.Customer.SingleOrDefaultAsync(c => c.Email == email);
            if (cus == null)
            {
                return BadRequest("Customer not found");
            }
            var aplan = await _context.Customersubscription.SingleOrDefaultAsync(obj => obj.Customer == email && obj.Status == "Active");
            if (aplan != null)
            {
                var asub = await _context.Subscriptionplan.SingleOrDefaultAsync(s => s.Subid == aplan.Subid);
                return cus.Accounttype.ToLower() != "paid" && aplan != null && asub.Type.ToLower() == "business";
            }
            return cus.Accountcategory.ToLower() == "business";
        }

        [Route("[action]/{email}/{oldpassword}/{newpassword}")]
        [HttpPost]
        public async Task<ActionResult> ChangePassword(string email, string oldpassword, string newpassword)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email && obj.Status != "Blocked");
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
                var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
                if (target == null)
                {
                    return NotFound("User not found");
                }
                string tk = Util.GenToken();
                Util.SendMail("Click <a href='https://www.smelms.com/customer/resetpassword?email=" + email + "&token=" + tk + "'>here</a> to reset your password.", "Password Reset", email);
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
                var cus = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email && obj.Status != "Blocked");
                cus.Password = Util.Encrypt(newpassword);
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
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
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
                    return BadRequest("Empty file");
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
        [HttpPost]
        public async Task<ActionResult> UploadCompanyLogo(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null || Request.Form.Files.Count == 0)
            {
                return BadRequest("User not found or request file missing");
            }

            if (target.Accountcategory.ToLower() == "personal")
            {
                return BadRequest("This customer doesn't have a business account");
            }
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "Clogos");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, email.ToLower() + ".png");
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return Ok();
                }
                else
                {
                    return BadRequest("Empty file");
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
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
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
        public async Task<ActionResult<string>> GetCompanyLogo(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            if (target.Accountcategory.ToLower() == "personal")
            {
                return BadRequest("This customer doesn't have a business account");
            }
            var folderName = Path.Combine("Res", "Clogos");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, email.ToLower() + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return Path.Combine(folderName, email.ToLower() + ".png"); ;
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsSubscriber(string email)
        {
            var cus = await _context.Customer.SingleOrDefaultAsync(c => c.Email == email);
            if (cus == null)
            {
                return BadRequest("Cusomter not found");
            }
            else
            {
                return cus.Accounttype.ToLower() != "paid";
            }
        }

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetDpRaw(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
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

        [Route("[action]/{email}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetCompanyLogoRaw(string email)
        {
            var target = await _context.Customer.SingleOrDefaultAsync(obj => obj.Email == email);
            if (target == null)
            {
                return NotFound();
            }
            if (target.Accountcategory.ToLower() == "personal")
            {

                return BadRequest("This customer doesn't have a business account");
            }
            var folderName = Path.Combine("Res", "Clogos");
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