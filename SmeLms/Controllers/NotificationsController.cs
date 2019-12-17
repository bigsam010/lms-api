using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        smelmsContext _context;
        public NotificationsController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Notifications[]>> Get()
        {
            return await _context.Notifications.ToArrayAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notifications>> Get(int id)
        {
            var target = await _context.Notifications.SingleOrDefaultAsync(obj => obj.Id == id);
            if (target == null)
            {
                return NotFound();
            }
            // target.Viewed = 1;
            // await _context.SaveChangesAsync();
            return target;
        }
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<ActionResult> SubjectObject(int id)
        {
            try
            {
                var target = await _context.Notifications.SingleOrDefaultAsync(obj => obj.Id == id);
                if (target == null)
                {
                    return BadRequest("Invalid notification id");
                }
                switch (target.Type.ToLower())
                {
                    case "purchase":

                        Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
                        MatchCollection emailMatches = emailRegex.Matches(target.Remark);
                        StringBuilder sb = new StringBuilder();
                        if (emailMatches.Count > 0)
                        {
                            Hashtable response = new Hashtable();
                            response["subject"] = emailMatches[0].Value;
                            response["object"] = target.Remark.Substring(target.Remark.LastIndexOf("[") + 1).Replace(']', ' ').Trim();
                            response["type"] = "class";
                            if (target.Remark.ToLower().Contains("purchased the course"))
                            {
                                response["type"] = "course";
                            }

                            return Ok(response);
                        }

                        return BadRequest("Could not get subject/object");
                    case "newcourse":
                        string[] results = Regex.Matches(target.Remark, @"\[(.+?)\]")
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value)
                            .ToArray();
                        if (results.Count() > 0)
                        {
                            Hashtable response = new Hashtable();
                            response["subject"] = _context.Beneficiary.SingleOrDefault(b => b.Email == target.Target).Addedby;
                            response["object"] = results[0];
                            return Ok(response);
                        }
                        return BadRequest("Could not get subject/object");
                    case "publish":
                        if (target.Remark.Contains("course"))
                        {
                            string[] results2 = Regex.Matches(target.Remark, @"\[(.+?)\]")
                            .Cast<Match>()
                             .Select(m => m.Groups[1].Value)
                            .ToArray();
                            if (results2.Count() > 0)
                            {
                                Hashtable response = new Hashtable();
                                response["subject"] = _context.Course.SingleOrDefault(b => b.Coursecode == results2[0]).Author;
                                response["object"] = results2[0];
                                response["type"] = "course";
                                return Ok(response);
                            }

                        }
                        else if (target.Remark.Contains("class"))
                        {
                            string[] results2 = Regex.Matches(target.Remark, @"\[(.+?)\]")
                                                   .Cast<Match>()
                                                    .Select(m => m.Groups[1].Value)
                                                   .ToArray();
                            if (results2.Count() > 0)
                            {
                                Hashtable response = new Hashtable();
                                response["subject"] = _context.Inclass.SingleOrDefault(b => b.Classid.ToString() == results2[0]).Createdby;
                                response["object"] = results2[0];
                                response["type"] = "class";
                                return Ok(response);
                            }


                        }
                        else if (target.Remark.Contains("blogpost"))
                        {
                            string[] results2 = Regex.Matches(target.Remark, @"\[(.+?)\]")
                                             .Cast<Match>()
                                              .Select(m => m.Groups[1].Value)
                                             .ToArray();
                            if (results2.Count() > 0)
                            {
                                Hashtable response = new Hashtable();
                                var bpost = _context.Blogpost.SingleOrDefault(b => b.Postid.ToString() == results2[0]);
                                response["subject"] = null;

                                if (bpost != null)
                                {
                                    response["subject"] = bpost.Author;
                                }

                                response["object"] = results2[0];
                                response["type"] = "blogpost";
                                return Ok(response);
                            }

                        }
                        return BadRequest("Could not get subject/object");
                    default:
                        return BadRequest("Invalid notification type");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex);
            }
        }
        [Route("[action]/{user}/{type}")]
        [HttpGet]
        public async Task<ActionResult<Notifications[]>> UserNotificationByType(string user, string type)
        {
            return await _context.Notifications.Where(un => un.Target == user && un.Type == type && un.Viewed == 0).ToArrayAsync();

        }

        [Route("[action]/{id}")]
        [HttpPut]
        public async Task<ActionResult> MarkViewed(int id)
        {
            var target = await _context.Notifications.SingleOrDefaultAsync(n => n.Id == id);
            target.Viewed = 1;
            await _context.SaveChangesAsync();
            return Ok();

        }

        [Route("[action]/{user}")]
        [HttpGet]
        public async Task<ActionResult<Notifications[]>> UserNotification(string user)
        {
            return await _context.Notifications.Where(un => un.Target == user && un.Viewed == 0).OrderByDescending(p => p.Notedate).ToArrayAsync();

        }

        [Route("[action]/{user}")]
        [HttpGet]
        public async Task<ActionResult<int>> UserNotificationCount(string user)
        {
            return await _context.Notifications.CountAsync(un => un.Target == user && un.Viewed == 0);

        }

        [Route("[action]/{beneficiary}")]
        [HttpGet]
        public async Task<ActionResult<bool>> NewCourseAdded(string beneficiary)
        {
            var target = await _context.Notifications.SingleOrDefaultAsync(un => un.Target == beneficiary && un.Type == "NewCourse" && un.Viewed == 0);
            return target != null;

        }

        [Route("[action]/{beneficiary}")]
        [HttpGet]
        public async Task<ActionResult<bool>> NewClassInviteSent(string beneficiary)
        {
            var target = await _context.Notifications.SingleOrDefaultAsync(un => un.Target == beneficiary && un.Type == "Invite" && un.Viewed == 0);
            return target != null;

        }

        [HttpPost]
        public async Task<ActionResult<Notifications>> Post([FromBody] Notifications obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            else
            {
                _context.Notifications.Add(obj);
                await _context.SaveChangesAsync();
                return Created("api/Notifications", obj);
            }
        }

        /*     [HttpPut ("{id}")]
            public async Task<ActionResult> Put (int id, [FromBody] Notifications obj) {
                var target = await _context.Notifications.SingleOrDefaultAsync (nobj => nobj.Id == id);
                if (target != null && ModelState.IsValid) {
                    _context.Entry (target).CurrentValues.SetValues (obj);
                    await _context.SaveChangesAsync ();
                    return Ok ();
                }
                return BadRequest ();
            } */

        /*   [HttpDelete ("{id}")]
          public async Task<ActionResult> Delete (int id) {
              var target = await _context.Notifications.SingleOrDefaultAsync (obj => obj.Id == id);
              if (target != null) {
                  _context.Notifications.Remove (target);
                  await _context.SaveChangesAsync ();
                  return Ok ();
              }
              return NotFound ();
          } */
    }
}