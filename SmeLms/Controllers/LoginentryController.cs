using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class LoginentryController : ControllerBase {
        smelmsContext _context;
        public LoginentryController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Loginentry[]>> Get () {
            return await _context.Loginentry.ToArrayAsync ();
        }

        [HttpGet ("{id}")]
        public async Task<ActionResult<Loginentry>> Get (int id) {
            var target = await _context.Loginentry.SingleOrDefaultAsync (obj => obj.Id == id);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Loginentry[]>> LoginHistory (string customer) {
            return await _context.Loginentry.Where (le => le.Client == customer).ToArrayAsync ();
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Loginentry>> LastLogin (string customer) {
            var llogin = await _context.Loginentry.Where (cl => cl.Client == customer).OrderByDescending (le => le.Logindate).FirstOrDefaultAsync ();
            if (llogin == null) {
                return NotFound ();
            }
            return llogin;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<int>> LoginTotal (string customer) {
            return await _context.Loginentry.CountAsync (le => le.Client == customer);
        }

        /* [HttpPost]
        public async Task<ActionResult<Loginentry>> Post ([FromBody] Loginentry obj) {
            if (!ModelState.IsValid) {
                return BadRequest ("Invalid model state");
            } else {
                obj.Logindate = DateTime.Now;
                _context.Loginentry.Add (obj);
                await _context.SaveChangesAsync ();
                return Created ("api/Loginentry", obj);
            }
        }*/

    }
}