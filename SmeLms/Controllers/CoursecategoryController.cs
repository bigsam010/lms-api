using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class CoursecategoryController : ControllerBase {
        smelmsContext _context;
        public CoursecategoryController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Coursecategory[]>> Get () {
            return await _context.Coursecategory.ToArrayAsync ();
        }

        [HttpGet ("{catid}")]
        public async Task<ActionResult<Coursecategory>> Get (int catid) {
            var target = await _context.Coursecategory.SingleOrDefaultAsync (obj => obj.Catid == catid);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [HttpGet ("[action]/{catid}")]
        public async Task<ActionResult<bool>> HasCourse (string catid) {
            var target = await _context.Coursecategory.SingleOrDefaultAsync (obj => obj.Catid == Convert.ToInt32 (catid));
            if (target == null) {
                return NotFound ();
            }
            return _context.Course.Where (obj => obj.Catid.Contains (catid)).ToArray ().Length > 0;

        }

        [HttpGet ("[action]/{catid}")]
        public async Task<ActionResult<bool>> HasClass (string catid) {
            var target = await _context.Coursecategory.SingleOrDefaultAsync (obj => obj.Catid == Convert.ToInt32 (catid));
            if (target == null) {
                return NotFound ();
            }
            return _context.Inclass.Where (obj => obj.Catid.Contains (catid)).ToArray ().Length > 0;

        }

        [HttpPost]
        public async Task<ActionResult<Coursecategory>> Post ([FromBody] Coursecategory obj) {
            if (!ModelState.IsValid) {
                return BadRequest ();
            } else {
                _context.Coursecategory.Add (obj);
                await _context.SaveChangesAsync ();
                return Created ("api/Coursecategory", obj);
            }
        }

        [HttpPut ("{catid}")]
        public async Task<ActionResult> Put (int catid, [FromBody] Coursecategory obj) {
            var target = await _context.Coursecategory.SingleOrDefaultAsync (nobj => nobj.Catid == catid);
            if (target != null && ModelState.IsValid) {
                _context.Entry (target).CurrentValues.SetValues (obj);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return BadRequest ();
        }

        [HttpDelete ("{catid}")]
        public async Task<ActionResult> Delete (int catid) {
            var target = await _context.Coursecategory.SingleOrDefaultAsync (obj => obj.Catid == catid);

            if (target != null) {

                if (_context.Course.Where (obj => obj.Catid.Contains (Convert.ToString (catid))).Count () > 0) {

                    return BadRequest ();
                }
                _context.Coursecategory.Remove (target);
                await _context.SaveChangesAsync ();
                return Ok ();
            }
            return NotFound ();
        }
    }
}