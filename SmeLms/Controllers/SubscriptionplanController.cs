using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class SubscriptionplanController : ControllerBase {
        smelmsContext _context;
        public SubscriptionplanController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Subscriptionplan[]>> Get () {
            return await _context.Subscriptionplan.ToArrayAsync ();
        }

        [Route ("[action]/{plan}")]
        [HttpGet]
        public async Task<ActionResult<Customer[]>> SubscribedTo (int plan) {
            var estudents = await _context.Customersubscription.Where (obj => obj.Subid == plan && obj.Status == "active").Select (obj => obj.Customer).ToListAsync ();
            return await _context.Customer.Where (c => estudents.Contains (c.Email)).ToArrayAsync ();

        }

        [Route ("[action]/{type}")]
        [HttpGet]
        public async Task<ActionResult<Subscriptionplan[]>> GetByType (string type) {
            return await _context.Subscriptionplan.Where (sp => sp.Type == type).ToArrayAsync ();
        }

        [Route ("[action]/{cycle}")]
        [HttpGet]
        public async Task<ActionResult<Subscriptionplan[]>> GetByCycle (string cycle) {
            return await _context.Subscriptionplan.Where (sp => sp.Cycle == cycle).ToArrayAsync ();
        }

        [Route ("[action]/{low}/{high}")]
        [HttpGet]
        public async Task<ActionResult<Subscriptionplan[]>> GetByPrice (decimal low, decimal high) {
            return await _context.Subscriptionplan.Where (sp => sp.Amount >= low && sp.Amount <= high).ToArrayAsync ();
        }

        [HttpGet ("{subid}")]
        public async Task<ActionResult<Subscriptionplan>> Get (int subid) {
            var target = await _context.Subscriptionplan.SingleOrDefaultAsync (obj => obj.Subid == subid);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [HttpPost]
        public async Task<ActionResult<Subscriptionplan>> Post ([FromBody] Subscriptionplan obj) {
            if (!ModelState.IsValid) {
                return BadRequest ();
            } else {
                _context.Subscriptionplan.Add (obj);
                await _context.SaveChangesAsync ();
                return Created ("api/Subscriptionplan", obj);
            }
        }

        [HttpPut ("{subid}")]
        public async Task<ActionResult> Put (int subid, [FromBody] Subscriptionplan obj) {
            var target = await _context.Subscriptionplan.SingleOrDefaultAsync (nobj => nobj.Subid == subid);
            if (target != null && ModelState.IsValid) {
                _context.Entry (target).CurrentValues.SetValues (obj);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return BadRequest ();
        }

        [HttpDelete ("{subid}")]
        public async Task<ActionResult> Delete (int subid) {
            var target = await _context.Subscriptionplan.SingleOrDefaultAsync (obj => obj.Subid == subid);
            if (target != null) {
                target.Status = "Suspended";
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return NotFound ();
        }

        [Route ("[action]/{subid}")]
        [HttpPost ]
        public async Task<ActionResult> Activate (int subid) {
            var target = await _context.Subscriptionplan.SingleOrDefaultAsync (obj => obj.Subid == subid);
            if (target != null) {
                target.Status = "Active";
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return NotFound ();
        }
    }
}