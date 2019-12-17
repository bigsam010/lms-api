using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class PlanchangerequestController : ControllerBase {
        smelmsContext _context;
        public PlanchangerequestController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Planchangerequest[]>> Get () {
            return await _context.Planchangerequest.ToArrayAsync ();
        }

        [Route ("[action]/{customer}")]
        [HttpGet]
        public async Task<ActionResult<Planchangerequest[]>> CustomerHistory (string customer) {
            return await _context.Planchangerequest.Where (c => c.Customer == customer).ToArrayAsync ();
        }

        [HttpGet ("{id}")]
        public async Task<ActionResult<Planchangerequest>> Get (int id) {
            var target = await _context.Planchangerequest.SingleOrDefaultAsync (obj => obj.Id == id);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        /*  [HttpPost]
          public async Task<ActionResult<Planchangerequest>> Post ([FromBody] Planchangerequest obj) {
              if (!ModelState.IsValid) {
                  return BadRequest ();
              } else {
                  _context.Planchangerequest.Add (obj);
                  await _context.SaveChangesAsync ();
                  return Created ("api/Planchangerequest", obj);
              }
          }

          [HttpPut ("{id}")]
          public async Task<ActionResult> Put (int id, [FromBody] Planchangerequest obj) {
              var target = await _context.Planchangerequest.SingleOrDefaultAsync (nobj => nobj.Id == id);
              if (target != null && ModelState.IsValid) {
                  _context.Entry (target).CurrentValues.SetValues (obj);
                  await _context.SaveChangesAsync ();
                  return Ok ();
              }
              return BadRequest ();
          }

          [HttpDelete ("{id}")]
          public async Task<ActionResult> Delete (int id) {
              var target = await _context.Planchangerequest.SingleOrDefaultAsync (obj => obj.Id == id);
              if (target != null) {
                  _context.Planchangerequest.Remove (target);
                  await _context.SaveChangesAsync ();
                  return Ok ();
              }
              return NotFound ();
          }*/
    }
}