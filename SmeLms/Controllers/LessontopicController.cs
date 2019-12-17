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
    public class LessontopicController : ControllerBase
    {
        smelmsContext _context;
        public LessontopicController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Lessontopic[]>> Get()
        {
            return await _context.Lessontopic.ToArrayAsync();
        }

        [HttpGet("{topicid}")]
        public async Task<ActionResult<Lessontopic>> Get(int topicid)
        {
            var target = await _context.Lessontopic.SingleOrDefaultAsync(obj => obj.Topicid == topicid);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [Route("[action]/{lessonid}")]
        [HttpGet]
        public async Task<ActionResult<Lessontopic[]>> GetLessonTopic(int lessonid)
        {
            return await _context.Lessontopic.Where(obj => obj.Lessonid == lessonid).OrderBy(lt => lt.Position).ToArrayAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Lessontopic>> Post([FromBody] Lessontopic obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid model state");
                }
                else
                {
                    if (_context.Lessontopic.SingleOrDefault(lt => lt.Lessonid == obj.Lessonid && lt.Position == obj.Position) != null)
                    {
                        return BadRequest("Invalid topic position. Position must be uique");
                    }
                    _context.Lessontopic.Add(obj);
                    await _context.SaveChangesAsync();
                    return Created("api/Lessontopic", obj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex);
            }
        }

        [HttpPut("{topicid}")]
        public async Task<ActionResult> Put(int topicid, [FromBody] Lessontopic obj)
        {
            try
            {
                var target = await _context.Lessontopic.SingleOrDefaultAsync(nobj => nobj.Topicid == topicid);
                if (target != null && ModelState.IsValid)
                {
                    //var prob = _context.Lessontopic.SingleOrDefault(lt => lt.Lessonid == obj.Lessonid && lt.Position == obj.Position);
                    //if (prob != null && prob.Topicid != target.Topicid)
                    //{
                    //    return BadRequest("Invalid topic position. Position must be uique");
                    //}
                    _context.Entry(target).CurrentValues.SetValues(obj);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex);
            }
            return BadRequest("Invalid topic id");
        }

        [HttpDelete("{topicid}")]
        public async Task<ActionResult> Delete(int topicid)
        {
            var target = await _context.Lessontopic.SingleOrDefaultAsync(obj => obj.Topicid == topicid);
            if (target != null)
            {
                _context.Lessontopic.Remove(target);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}