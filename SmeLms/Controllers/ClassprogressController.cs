using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers {
    [Route ("api/[controller]")]
    [ApiController] public class ClassprogressController : ControllerBase {
        smelmsContext _context;
        public ClassprogressController (smelmsContext _context) {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Classprogress[]>> Get () {
            return await _context.Classprogress.ToArrayAsync ();
        }
        string email2, subject;
        string[] bccs;
        void dispatchMail () {
            Util.SendMail (email2, subject, bccs);
        }
        /*  [Route ("[action]/{topicid}")]
         [HttpGet]
         public async Task<ActionResult<Topiccontent[]>> AllTopic (int topicid) {
             var contents = _context.Topiccontent.Where (tc => tc.Topicid == topicid).Select (tc => tc.Topicid).ToList ();
             return await _context.Topiccontent.Where (cp => contents.Contains (cp.Topicid)).ToArrayAsync ();
         } */

        [HttpGet ("{id}")]
        public async Task<ActionResult<Classprogress>> Get (int id) {
            var target = await _context.Classprogress.SingleOrDefaultAsync (obj => obj.Id == id);
            if (target == null) {
                return NotFound ();
            }
            return target;
        }

        [Route ("[action]/{class}/{content}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsContentCompleted (int Class, int content) {
            var target = await _context.Classprogress.SingleOrDefaultAsync (obj => obj.Classid == Class && obj.Contentcompleted == content);
            return target != null;
        }

        [Route ("[action]/{class}/{topic}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsTopicCompleted (int Class, int topic) {

            var contents = await _context.Topiccontent.Where (tc => tc.Topicid == topic).Select (tc => tc.Contentid).ToListAsync ();
            var cconts = await _context.Classprogress.Where (cp => cp.Classid == Class && contents.Contains (cp.Contentcompleted)).ToListAsync ();
            return contents.Count == cconts.Count;

        }

        [Route ("[action]/{class}/{lesson}")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsLessonCompleted (int Class, int lesson) {
            int cTopics = 0;
            var topics = await _context.Lessontopic.Where (lt => lt.Lessonid == lesson).Select (lt => lt.Topicid).ToListAsync ();
            foreach (var t in topics) {
                var contents = await _context.Topiccontent.Where (tc => tc.Topicid == t).Select (tc => tc.Contentid).ToListAsync ();
                var cconts = await _context.Classprogress.Where (cp => cp.Classid == Class && contents.Contains (cp.Contentcompleted)).ToListAsync ();
                if (contents.Count == cconts.Count) {
                    cTopics++;
                }

            }
            return topics.Count == cTopics;

        }

        [Route ("[action]/{class}")]
        [HttpGet]
        public async Task<ActionResult<decimal>> CompletionPercentage (int Class) {
            var cClass = await _context.Customerclass.SingleOrDefaultAsync (cc => cc.Classid == Class);
            if (cClass == null) {
                return BadRequest ("Invalid class");
            }
            var lessons = await _context.Courselesson.Where (cl => cl.Coursecode == cClass.Coursecode).Select (cl => cl.Lessonid).ToListAsync ();
            if (lessons.Count == 0) {
                return BadRequest ("Empty lesson list for this class");
            }
            int contentTotal = 0;
            int completedContent = 0;

            foreach (var l in lessons) {

                var topics = await _context.Lessontopic.Where (lt => lt.Lessonid == l).Select (lt => lt.Topicid).ToListAsync ();
                foreach (var t in topics) {
                    var contents = await _context.Topiccontent.Where (tc => tc.Topicid == t).Select (tc => tc.Contentid).ToListAsync ();
                    foreach (int c in contents) {
                        contentTotal++;
                        if (await _context.Classprogress.SingleOrDefaultAsync (cp => cp.Classid == Class && cp.Contentcompleted == c) != null) {
                            completedContent++;
                        }
                    }

                }

            }

            decimal d = Convert.ToDecimal ((Convert.ToDecimal (completedContent) / Convert.ToDecimal (contentTotal)) * Convert.ToDecimal (100));
            return Math.Round (d, 2);
        }

        [Route ("[action]/{topic}")]
        [HttpGet]
        public async Task<ActionResult<int>> ContentTotal (int topic) {
            var t = await _context.Lessontopic.SingleOrDefaultAsync (lt => lt.Topicid == topic);
            if (t == null) {
                return BadRequest ("Invalid topic");
            }
            return _context.Topiccontent.Where (tc => tc.Topicid == topic).Count ();
        }

        [Route ("[action]/{class}/{topic}")]
        [HttpGet]
        public async Task<ActionResult<int>> ContentCompletedTotal (int Class, int topic) {

            var contents = await _context.Topiccontent.Where (tc => tc.Topicid == topic).Select (tc => tc.Contentid).ToListAsync ();
            var cconts = await _context.Classprogress.Where (cp => cp.Classid == Class && contents.Contains (cp.Contentcompleted)).ToListAsync ();
            return cconts.Count;

        }

        [Route ("[action]/{lesson}")]
        [HttpGet]
        public async Task<ActionResult<int>> TopicTotal (int lesson) {
            var l = await _context.Courselesson.SingleOrDefaultAsync (cl => cl.Lessonid == lesson);
            if (l == null) {
                return BadRequest ("Invalid lesson");
            }

            return await _context.Lessontopic.Where (lt => lt.Lessonid == lesson).Select (lt => lt.Topicid).CountAsync ();

        }

        [Route ("[action]/{class}/{lesson}")]
        [HttpGet]
        public async Task<ActionResult<int>> TopicCompletedTotal (int Class, int lesson) {
            var l = await _context.Courselesson.SingleOrDefaultAsync (cl => cl.Lessonid == lesson);
            if (l == null) {
                return BadRequest ("Invalid lesson");
            }
            int cTopics = 0;
            var topics = await _context.Lessontopic.Where (lt => lt.Lessonid == lesson).Select (lt => lt.Topicid).ToListAsync ();
            foreach (var t in topics) {
                var contents = await _context.Topiccontent.Where (tc => tc.Topicid == t).Select (tc => tc.Contentid).ToListAsync ();
                var cconts = await _context.Classprogress.Where (cp => cp.Classid == Class && contents.Contains (cp.Contentcompleted)).ToListAsync ();
                if (contents.Count == cconts.Count) {
                    cTopics++;
                }

            }
            return cTopics;
        }

        [HttpPost]
        public async Task<ActionResult<Classprogress>> Post ([FromBody] Classprogress obj) {
            if (!ModelState.IsValid) {
                return BadRequest ("Invalid model state");
            } else {
                var prog = await _context.Classprogress.SingleOrDefaultAsync (cp => cp.Classid == obj.Classid && cp.Contentcompleted == obj.Contentcompleted);
                if (prog != null) {
                    return BadRequest ("Content already marked completed for this class");
                }
                _context.Classprogress.Add (obj);

                await _context.SaveChangesAsync ();
                var cClass = await _context.Customerclass.SingleOrDefaultAsync (cc => cc.Classid == obj.Classid);
                if (cClass == null) {
                    return BadRequest ("Invalid class");
                }
                var lessons = await _context.Courselesson.Where (cl => cl.Coursecode == cClass.Coursecode).Select (cl => cl.Lessonid).ToListAsync ();
                if (lessons.Count == 0) {
                    return BadRequest ("Empty lesson list for this class");
                }
                int contentTotal = 0;
                int completedContent = 0;

                foreach (var l in lessons) {

                    var topics = await _context.Lessontopic.Where (lt => lt.Lessonid == l).Select (lt => lt.Topicid).ToListAsync ();
                    foreach (var t in topics) {
                        var contents = await _context.Topiccontent.Where (tc => tc.Topicid == t).Select (tc => tc.Contentid).ToListAsync ();
                        foreach (int c in contents) {
                            contentTotal++;
                            if (await _context.Classprogress.SingleOrDefaultAsync (cp => cp.Classid == obj.Classid && cp.Contentcompleted == c) != null) {
                                completedContent++;
                            }
                        }

                    }

                }

                decimal d = Convert.ToDecimal ((Convert.ToDecimal (completedContent) / Convert.ToDecimal (contentTotal)) * Convert.ToDecimal (100));
                if (d >= Convert.ToDecimal (100)) {
                    var cc = await _context.Customerclass.SingleOrDefaultAsync (cp => cp.Classid == obj.Classid);
                    if (cc != null) {
                        cc.Status = "Completed";
                        cc.Enddate = DateTime.Now;
                        await _context.SaveChangesAsync ();
                        var bb = await _context.Beneficiary.SingleOrDefaultAsync (b => b.Email == cc.Customer);
                        if (bb != null) {
                            var crs = await _context.Course.SingleOrDefaultAsync (c => c.Coursecode == cc.Coursecode);
                            var ben = await _context.Customer.SingleOrDefaultAsync (c => c.Email == cc.Customer);
                            email2 = "Your staff " + ben.Lastname + " " + ben.Firstname + " just completed the course: " + crs.Title + " [" + crs.Coursecode + "]";
                            subject = "Course completion Notification";
                            bccs = new string[] { bb.Addedby };
                            ThreadStart ts = new ThreadStart (dispatchMail);
                            Thread t1 = new Thread (ts);
                            t1.Start ();
                        }
                    }
                }
                return Created ("api/Classprogress", obj);
            }
        }

        [Route ("[action]/{course}")]
        [HttpGet]
        public async Task<ActionResult<int>> LessonTotal (string course) {
            var c = await _context.Course.SingleOrDefaultAsync (cl => cl.Coursecode == course);
            if (c == null) {
                return BadRequest ("Invalid course");
            }

            return await _context.Courselesson.Where (lt => lt.Coursecode == course).CountAsync ();

        }

        /*         [HttpPut ("{id}")]
                public async Task<ActionResult> Put (int id, [FromBody] Classprogress obj) {
                    var target = await _context.Classprogress.SingleOrDefaultAsync (nobj => nobj.Id == id);
                    if (target != null && ModelState.IsValid) {
                        _context.Entry (target).CurrentValues.SetValues (obj);
                        await _context.SaveChangesAsync ();
                        return Ok ();
                    }
                    return BadRequest ();
                }

                [HttpDelete ("{id}")]
                public async Task<ActionResult> Delete (int id) {
                    var target = await _context.Classprogress.SingleOrDefaultAsync (obj => obj.Id == id);
                    if (target != null) {
                        _context.Classprogress.Remove (target);
                        await _context.SaveChangesAsync ();
                        return Ok ();
                    }
                    return NotFound ();
                } */
    }
}