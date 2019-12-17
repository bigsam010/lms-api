using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class CustomercardController : ControllerBase {
        smelmsContext _context;
        public CustomercardController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Customercard[]>> Get () {
            return await _context.Customercard.ToArrayAsync ();
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Customercard>> MyCard (string customer) {
            var ccard = await _context.Customercard.SingleOrDefaultAsync (obj => obj.Customer == customer);
            if (ccard == null) {
                return NotFound ();
            }
            return ccard;
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<bool>> HasCard (string customer) {

            var ccard = await _context.Customercard.SingleOrDefaultAsync (cc => cc.Customer == customer);
            return ccard != null;
        }

        [HttpGet ("{cardnumber}")]
        public async Task<ActionResult<Customercard>> Get (string cardnumber) {
            var target = await _context.Customercard.SingleOrDefaultAsync (obj => obj.Cardnumber == cardnumber);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [HttpPost]
        public async Task<ActionResult<Customercard>> Post ([FromBody] Customercard obj) {
            if (!ModelState.IsValid) {
                return BadRequest ("Invalid model state");
            }
            if (await _context.Customer.SingleOrDefaultAsync (c => c.Email == obj.Customer) == null) {
                return BadRequest ("Invalid customer");
            }
            if (await _context.Customercard.SingleOrDefaultAsync (t => t.Cardnumber == obj.Cardnumber) != null) {
                return BadRequest ("Card already exist");
            }
            if (await _context.Customercard.SingleOrDefaultAsync (cc => cc.Customer == obj.Customer) != null) {
                return BadRequest ("A card is already assigned to this customer.");
            } else {
                _context.Customercard.Add (obj);
                await _context.SaveChangesAsync ();
                return Created ("api/Customercard", obj);
            }
        }

        [HttpPut ("{cardnumber}")]
        public async Task<ActionResult> Put (string cardnumber, [FromBody] Customercard obj) {
            var target = await _context.Customercard.SingleOrDefaultAsync (nobj => nobj.Cardnumber == cardnumber);
            if (target != null && ModelState.IsValid) {
                _context.Entry (target).CurrentValues.SetValues (obj);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return BadRequest ();
        }

        [HttpDelete ("{cardnumber}")]
        public async Task<ActionResult> Delete (string cardnumber) {
            var target = await _context.Customercard.SingleOrDefaultAsync (obj => obj.Cardnumber == cardnumber);
            if (target != null) {
                _context.Customercard.Remove (target);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return NotFound ();
        }
    }
}