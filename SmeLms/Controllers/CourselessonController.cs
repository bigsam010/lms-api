using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourselessonController : ControllerBase
    {
        smelmsContext _context;
        public CourselessonController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Courselesson[]>> Get()
        {
            return await _context.Courselesson.ToArrayAsync();
        }

        [HttpGet("{lessonid}")]
        public async Task<ActionResult<Courselesson>> Get(int lessonid)
        {
            var target = await _context.Courselesson.SingleOrDefaultAsync(obj => obj.Lessonid == lessonid);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [HttpGet("[action]/{coursecode}")]
        public async Task<ActionResult<Courselesson[]>> GetCourseLesson(string coursecode)
        {
            return await _context.Courselesson.Where(obj => obj.Coursecode == coursecode).OrderBy(cl=>cl.Position). ToArrayAsync();

        }

        [HttpPost]
        public async Task<ActionResult<Courselesson>> Post([FromBody] Courselesson obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }
            else
            {
                if (_context.Courselesson.SingleOrDefault(cl => cl.Coursecode == obj.Coursecode && cl.Position == obj.Position) != null)
                {
                    return BadRequest("Invalid lesson position. Postion must be unique");
                }
                _context.Courselesson.Add(obj);
                await _context.SaveChangesAsync();
                return Created("api/Courselesson", obj);
            }
        }

        [HttpPut("{lessonid}")]
        public async Task<ActionResult> Put(int lessonid, [FromBody] Courselesson obj)
        {
            var target = await _context.Courselesson.SingleOrDefaultAsync(nobj => nobj.Lessonid == lessonid);
            if (target != null && ModelState.IsValid)
            {
                //var prob = _context.Courselesson.SingleOrDefault(cl => cl.Coursecode == obj.Coursecode && cl.Position == obj.Position);
                //if (prob != null && prob.Lessonid != target.Lessonid)
                //{
                //    return BadRequest("Position already taken");
                //}
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Invalid lesson position. Postion must be unique");
        }

        [HttpDelete("{lessonid}")]
        public async Task<ActionResult> Delete(int lessonid)
        {
            var target = await _context.Courselesson.SingleOrDefaultAsync(obj => obj.Lessonid == lessonid);
            if (target != null)
            {
                _context.Courselesson.Remove(target);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}