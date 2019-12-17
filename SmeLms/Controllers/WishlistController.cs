using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class WishlistController : ControllerBase {
        smelmsContext _context;
        public WishlistController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet ("{customer}")]
        public async Task<ActionResult<Wishlist[]>> Get (string customer) {
            return await _context.Wishlist.Where (obj => obj.Customer == customer).ToArrayAsync ();
        }

        [Route ("[action]/{course}")]
        [HttpGet]
        public async Task<ActionResult<Wishlist[]>> WishlistedBy (string course) {
            return await _context.Wishlist.Where (obj => obj.Coursecode == course).ToArrayAsync ();
        }

        [HttpPost ("{customer}/{course}")]
        public async Task<ActionResult<Wishlist>> Post (string customer, string course) {
            var target = await _context.Wishlist.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Coursecode == course);
            if (target != null) {
                return BadRequest ("Course already wishlisted by this customer");
            }
            if (!ModelState.IsValid) {
                return BadRequest ("Invalid model state");
            } else {
                var obj = new Wishlist {
                    Customer = customer,
                    Coursecode = course
                };
                _context.Wishlist.Add (obj);
                await _context.SaveChangesAsync ();
                return Created ("api/Wishlist", obj);
            }
        }

        [HttpDelete ("{customer}/{course}")]
        public async Task<ActionResult> Delete (string customer, string course) {
            var target = await _context.Wishlist.SingleOrDefaultAsync (obj => obj.Customer == customer && obj.Coursecode == course);
            if (target != null) {
                _context.Wishlist.Remove (target);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return NotFound ();
        }
    }
}