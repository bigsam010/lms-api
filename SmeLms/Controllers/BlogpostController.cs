using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogpostController : ControllerBase
    {
        smelmsContext _context;
        public BlogpostController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult> Get(int pageNo = 1, int pageSize = 10)
        {
            try
            {
                int skip = (pageNo - 1) * pageSize;
                int total = _context.Blogpost.Count();
                var records = await _context.Blogpost.OrderByDescending(pb => pb.Publisheddate).Skip(skip).Take(pageSize).ToListAsync();
                List<AdvBlogpost> response = new List<AdvBlogpost>();
                foreach (Blogpost b in records)
                {
                    AdvBlogpost abp = new AdvBlogpost(b);
                    var atName = await _context.Users.SingleOrDefaultAsync(u => u.Email == b.Author);
                    if (atName != null)
                    {
                        abp.Authorname = atName.Firstname + " " + atName.Lastname;
                    }
                    var folderName = Path.Combine("Res", "BlogThumbnails");
                    abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                    response.Add(abp);
                }
                return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex);
            }
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> Search(string searchtext)
        {
            return Ok(await _context.Blogpost.Where(bp => bp.Caption.Contains(searchtext)).ToListAsync());
        }

        string email, subject;
        string[] bccs;
        void dispatchMail()
        {
            Util.SendMail(email, subject, bccs);
        }



        [HttpGet("{postid}")]
        public async Task<ActionResult<Blogpost>> Get(int postid)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == postid);
            if (target == null)
            {
                return BadRequest("Invalid post");
            }

            return target;
        }
        [Route("[action]/{postid}")]
        [HttpPost]
        public async Task<ActionResult> IncreaseShares(int postid)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == postid);
            if (target == null)
            {
                return BadRequest("Invaid post");
            }
            target.Shares++;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [Route("[action]/{postid}")]
        [HttpPost]
        public async Task<ActionResult> IncreaseViews(int postid)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == postid);
            if (target == null)
            {
                return BadRequest("Invaid post");
            }
            target.Views++;
            await _context.SaveChangesAsync();
            return Ok();
        }
        private class AdvBlogpost : Blogpost
        {
            public string Authorname { set; get; }
            public string Thumbnail { set; get; }
            public AdvBlogpost(Blogpost post)
            {
                this.Author = post.Author;
                this.Caption = post.Caption;
                this.Content = post.Content;
                this.Postid = post.Postid;
                this.Publisheddate = post.Publisheddate;
                this.Status = post.Status;
                this.Tag = post.Tag;
                this.Views = post.Views;
                this.Shares = post.Shares;
            }

        }
        [Route("[action]/{author}")]
        [HttpGet]
        public async Task<ActionResult> GetByAuthor(string author, int pageNo = 1, int pageSize = 10)
        {
            int skip = (pageNo - 1) * pageSize;
            var atName = await _context.Users.SingleOrDefaultAsync(u => u.Email == author);
            int total = _context.Blogpost.Where(bp => bp.Author == author).Count();
            var records = await _context.Blogpost.Where(bp => bp.Author == author).ToListAsync();
            List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
            foreach (Blogpost b in records)
            {
                AdvBlogpost abp = new AdvBlogpost(b);
                abp.Authorname = atName.Firstname + " " + atName.Lastname;
                var folderName = Path.Combine("Res", "BlogThumbnails");
                abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                advPosts.Add(abp);
            }
            var response = advPosts.OrderByDescending(p => p.Publisheddate).Skip(skip).Take(pageSize);

            return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortByViews(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            try
            {
                if (filtertype == null)
                {
                    if (filtervalue != null)
                    {
                        return BadRequest("Filter type is required");
                    }
                    int skip = (pageNo - 1) * pageSize;
                    int total = _context.Blogpost.Count();
                    var records = await _context.Blogpost.ToListAsync();
                    List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
                    foreach (Blogpost b in records)
                    {
                        var atName = _context.Users.SingleOrDefault(u => u.Email == b.Author);
                        AdvBlogpost abp = new AdvBlogpost(b);
                        abp.Authorname = atName.Firstname + " " + atName.Lastname;
                        var folderName = Path.Combine("Res", "BlogThumbnails");
                        abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                        advPosts.Add(abp);
                    }
                    var response = advPosts.OrderBy(p => p.Views).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advPosts.OrderByDescending(p => p.Views).Skip(skip).Take(pageSize);
                    }
                    if (author != null)
                    {
                        total = _context.Blogpost.Where(p => p.Author == author).Count();
                        response = advPosts.Where(p => p.Author.ToLower() == author.ToLower()).OrderBy(p => p.Views).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advPosts.Where(p => p.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Views).Skip(skip).Take(pageSize);

                        }

                    }
                    return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));
                }
                else
                {
                    if (filtervalue == null)
                    {
                        return BadRequest("Filter value is required");
                    }
                    switch (filtertype.ToLower())
                    {
                        case "author":
                            var atName = _context.Users.SingleOrDefault(u => u.Email == filtervalue);
                            int skip = (pageNo - 1) * pageSize;
                            int total = _context.Blogpost.Where(p => p.Author == filtervalue).Count();
                            var records = await _context.Blogpost.Where(p => p.Author == filtervalue).ToListAsync();
                            List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
                            foreach (Blogpost b in records)
                            {

                                AdvBlogpost abp = new AdvBlogpost(b);
                                abp.Authorname = atName.Firstname + " " + atName.Lastname;
                                var folderName = Path.Combine("Res", "BlogThumbnails");
                                abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                                advPosts.Add(abp);
                            }
                            var response = advPosts.OrderBy(p => p.Views).Skip(skip).Take(pageSize);
                            if (mode != "asc")
                            {
                                response = advPosts.OrderByDescending(p => p.Views).Skip(skip).Take(pageSize);
                            }
                            return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));

                        case "status":
                            List<string> statuses = new List<string>()
                        {
                            "published","indraft"
                        };
                            if (!statuses.Contains(filtervalue.ToLower()))
                            {
                                return BadRequest("Invalid blog post status");
                            }
                            int skip2 = (pageNo - 1) * pageSize;
                            int total2 = _context.Blogpost.Where(p => p.Status == filtervalue).Count();
                            var records2 = await _context.Blogpost.Where(p => p.Status == filtervalue).ToListAsync();
                            List<AdvBlogpost> advPosts2 = new List<AdvBlogpost>();
                            foreach (Blogpost b in records2)
                            {
                                var atName2 = _context.Users.SingleOrDefault(u => u.Email == b.Author);
                                AdvBlogpost abp = new AdvBlogpost(b);
                                abp.Authorname = atName2.Firstname + " " + atName2.Lastname;
                                var folderName = Path.Combine("Res", "BlogThumbnails");
                                abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                                advPosts2.Add(abp);
                            }
                            var response2 = advPosts2.OrderBy(p => p.Views).Skip(skip2).Take(pageSize);
                            if (mode != "asc")
                            {
                                response2 = advPosts2.OrderByDescending(p => p.Views).Skip(skip2).Take(pageSize);
                            }
                            if (author != null)
                            {
                                total2 = _context.Blogpost.Where(p => p.Status == filtervalue && p.Author == author).Count();
                                response2 = advPosts2.Where(p => p.Author.ToLower() == author.ToLower()).OrderBy(p => p.Views).Skip(skip2).Take(pageSize);
                                if (mode != "asc")
                                {
                                    response2 = advPosts2.Where(p => p.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Views).Skip(skip2).Take(pageSize);
                                }
                            }
                            return Ok(new PagedResult<AdvBlogpost>(response2, pageNo, pageSize, total2));
                        default:
                            return BadRequest("Invalid filter type");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error:" + ex);
            }

        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortAlphabetically(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            try
            {
                if (filtertype == null)
                {
                    if (filtervalue != null)
                    {
                        return BadRequest("Filter type is required");
                    }
                    int skip = (pageNo - 1) * pageSize;
                    int total = _context.Blogpost.Count();
                    var records = await _context.Blogpost.ToListAsync();
                    List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
                    foreach (Blogpost b in records)
                    {
                        var atName = _context.Users.SingleOrDefault(u => u.Email == b.Author);
                        AdvBlogpost abp = new AdvBlogpost(b);
                        abp.Authorname = atName.Firstname + " " + atName.Lastname;
                        var folderName = Path.Combine("Res", "BlogThumbnails");
                        abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                        advPosts.Add(abp);
                    }
                    var response = advPosts.OrderBy(p => p.Caption).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advPosts.OrderByDescending(p => p.Caption).Skip(skip).Take(pageSize);
                    }
                    if (author != null)
                    {
                        total = _context.Blogpost.Where(b => b.Author == author).Count();
                        response = advPosts.Where(b => b.Author.ToLower() == author.ToLower()).OrderBy(p => p.Caption).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advPosts.Where(b => b.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Caption).Skip(skip).Take(pageSize);
                        }
                    }
                    return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));
                }
                else
                {
                    if (filtervalue == null)
                    {
                        return BadRequest("Filter value is required");
                    }
                    switch (filtertype.ToLower())
                    {
                        case "author":
                            var atName = _context.Users.SingleOrDefault(u => u.Email == filtervalue);
                            int skip = (pageNo - 1) * pageSize;
                            int total = _context.Blogpost.Where(p => p.Author == filtervalue).Count();
                            var records = await _context.Blogpost.Where(p => p.Author == filtervalue).ToListAsync();
                            List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
                            foreach (Blogpost b in records)
                            {

                                AdvBlogpost abp = new AdvBlogpost(b);
                                abp.Authorname = atName.Firstname + " " + atName.Lastname;
                                var folderName = Path.Combine("Res", "BlogThumbnails");
                                abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                                advPosts.Add(abp);
                            }
                            var response = advPosts.OrderBy(p => p.Caption).Skip(skip).Take(pageSize);
                            if (mode != "asc")
                            {
                                response = advPosts.OrderByDescending(p => p.Caption).Skip(skip).Take(pageSize);
                            }
                            if (author != null)
                            {
                                total = _context.Blogpost.Where(b => b.Author == author).Count();
                                response = advPosts.Where(b => b.Author.ToLower() == author.ToLower()).OrderBy(p => p.Caption).Skip(skip).Take(pageSize);
                                if (mode != "asc")
                                {
                                    response = advPosts.Where(b => b.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Caption).Skip(skip).Take(pageSize);
                                }
                            }
                            return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));

                        case "status":
                            List<string> statuses = new List<string>()
                        {
                            "published","indraft"
                        };
                            if (!statuses.Contains(filtervalue.ToLower()))
                            {
                                return BadRequest("Invalid blog post status");
                            }
                            int skip2 = (pageNo - 1) * pageSize;
                            int total2 = _context.Blogpost.Where(p => p.Status == filtervalue).Count();
                            var records2 = await _context.Blogpost.Where(p => p.Status == filtervalue).ToListAsync();
                            List<AdvBlogpost> advPosts2 = new List<AdvBlogpost>();
                            foreach (Blogpost b in records2)
                            {
                                var atName2 = _context.Users.SingleOrDefault(u => u.Email == b.Author);
                                AdvBlogpost abp = new AdvBlogpost(b);
                                abp.Authorname = atName2.Firstname + " " + atName2.Lastname;
                                var folderName = Path.Combine("Res", "BlogThumbnails");
                                abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                                advPosts2.Add(abp);
                            }
                            var response2 = advPosts2.OrderBy(p => p.Caption).Skip(skip2).Take(pageSize);
                            if (mode != null)
                            {
                                response2 = advPosts2.OrderByDescending(p => p.Caption).Skip(skip2).Take(pageSize);
                            }
                            if (author != null)
                            {
                                total2 = _context.Blogpost.Where(p => p.Status == filtervalue && p.Author == author).Count();
                                response2 = advPosts2.Where(p => p.Author.ToLower() == author.ToLower()).OrderBy(p => p.Caption).Skip(skip2).Take(pageSize);
                                if (mode != "asc")
                                {
                                    response2 = advPosts2.Where(p => p.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Caption).Skip(skip2).Take(pageSize);
                                }
                            }
                            return Ok(new PagedResult<AdvBlogpost>(response2, pageNo, pageSize, total2));
                        default:
                            return BadRequest("Invalid filter type");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error:" + ex);
            }

        }


        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> SortByShares(string filtertype = null, string filtervalue = null, string author = null, string mode = "asc", int pageNo = 1, int pageSize = 10)
        {
            if (mode.ToLower() != "asc" && mode.ToLower() != "desc")
            {
                return BadRequest("Invalid sort mode");
            }
            if (filtertype == null)
            {
                if (filtervalue != null)
                {
                    return BadRequest("Filter type is required");
                }
                int skip = (pageNo - 1) * pageSize;
                int total = _context.Blogpost.Count();
                var records = await _context.Blogpost.ToListAsync();
                List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
                foreach (Blogpost b in records)
                {
                    var atName = _context.Users.SingleOrDefault(u => u.Email == b.Author);
                    AdvBlogpost abp = new AdvBlogpost(b);
                    abp.Authorname = atName.Firstname + " " + atName.Lastname;
                    var folderName = Path.Combine("Res", "BlogThumbnails");
                    abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                    advPosts.Add(abp);
                }
                var response = advPosts.OrderBy(p => p.Shares).Skip(skip).Take(pageSize);
                if (mode != "asc")
                {
                    response = advPosts.OrderByDescending(p => p.Shares).Skip(skip).Take(pageSize);
                }
                if (author != null)
                {
                    total = _context.Blogpost.Where(p => p.Author == author).Count();
                    response = advPosts.Where(p => p.Author.ToLower() == author.ToLower()).OrderBy(p => p.Shares).Skip(skip).Take(pageSize);
                    if (mode != "asc")
                    {
                        response = advPosts.Where(p => p.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Shares).Skip(skip).Take(pageSize);
                    }
                }
                return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));
            }
            else
            {
                if (filtervalue == null)
                {
                    return BadRequest("Filter value is required");
                }
                switch (filtertype.ToLower())
                {
                    case "author":
                        var atName = _context.Users.SingleOrDefault(u => u.Email == filtervalue);
                        int skip = (pageNo - 1) * pageSize;
                        int total = _context.Blogpost.Where(p => p.Author == filtervalue).Count();
                        var records = await _context.Blogpost.Where(p => p.Author == filtervalue).ToListAsync();
                        List<AdvBlogpost> advPosts = new List<AdvBlogpost>();
                        foreach (Blogpost b in records)
                        {

                            AdvBlogpost abp = new AdvBlogpost(b);
                            abp.Authorname = atName.Firstname + " " + atName.Lastname;
                            var folderName = Path.Combine("Res", "BlogThumbnails");
                            abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                            advPosts.Add(abp);
                        }

                        var response = advPosts.OrderBy(p => p.Shares).Skip(skip).Take(pageSize);
                        if (mode != "asc")
                        {
                            response = advPosts.OrderByDescending(p => p.Shares).Skip(skip).Take(pageSize);
                        }
                        return Ok(new PagedResult<AdvBlogpost>(response, pageNo, pageSize, total));

                    case "status":
                        List<string> statuses = new List<string>()
                        {
                            "published","indraft"
                        };
                        if (!statuses.Contains(filtervalue.ToLower()))
                        {
                            return BadRequest("Invalid blog post status");
                        }
                        int skip2 = (pageNo - 1) * pageSize;
                        int total2 = _context.Blogpost.Where(p => p.Status == filtervalue).Count();
                        var records2 = await _context.Blogpost.Where(p => p.Status == filtervalue).ToListAsync();
                        List<AdvBlogpost> advPosts2 = new List<AdvBlogpost>();
                        foreach (Blogpost b in records2)
                        {
                            var atName2 = _context.Users.SingleOrDefault(u => u.Email == b.Author);
                            AdvBlogpost abp = new AdvBlogpost(b);
                            abp.Authorname = atName2.Firstname + " " + atName2.Lastname;
                            var folderName = Path.Combine("Res", "BlogThumbnails");
                            abp.Thumbnail = Path.Combine(folderName, b.Postid + ".png");
                            advPosts2.Add(abp);
                        }
                        var response2 = advPosts2.OrderBy(p => p.Shares).Skip(skip2).Take(pageSize);
                        if (mode != "asc")
                        {
                            response2 = advPosts2.OrderByDescending(p => p.Shares).Skip(skip2).Take(pageSize);
                        }
                        if (author != null)
                        {
                            total2 = _context.Blogpost.Where(p => p.Status == filtervalue && p.Author == author).Count();
                            response2 = advPosts2.Where(p => p.Author.ToLower() == author.ToLower()).OrderBy(p => p.Shares).Skip(skip2).Take(pageSize);
                            if (mode != "asc")
                            {
                                response2 = advPosts2.Where(p => p.Author.ToLower() == author.ToLower()).OrderByDescending(p => p.Shares).Skip(skip2).Take(pageSize);
                            }
                        }
                        return Ok(new PagedResult<AdvBlogpost>(response2, pageNo, pageSize, total2));
                    default:
                        return BadRequest("Invalid filter type");
                }
            }

        }

        [HttpPost]
        public async Task<ActionResult<Blogpost>> Post([FromBody] Blogpost obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }
            else
            {
                _context.Blogpost.Add(obj);
                await _context.SaveChangesAsync();
                string[] aemails = _context.Users.Where(u => u.Role == "Admin").Select(u => u.Email).ToArray();
                var pid = _context.Blogpost.LastOrDefault().Postid;
                foreach (var e in aemails)
                {
                    var note = new Notifications()
                    {
                        Type = "Publish",
                        Target = e,
                        Remark = "New blogpost titled : " + obj.Caption + " [" + pid + "] just got published."
                    };
                    _context.Notifications.Add(note);
                }
                await _context.SaveChangesAsync();
                email = "New blogpost titled : " + obj.Caption + " just got published.";
                subject = "Blogpost Publish Alert";
                bccs = aemails;
                ThreadStart ts = new ThreadStart(dispatchMail);
                Thread t1 = new Thread(ts);
                t1.Start();
                return Created("api/Blogpost", obj);
            }
        }

        [HttpPut("{postid}")]
        public async Task<ActionResult> Put(int postid, [FromBody] Blogpost obj)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(nobj => nobj.Postid == postid);
            if (target != null && ModelState.IsValid)
            {
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{postid}")]
        public async Task<ActionResult> Delete(int postid)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == postid);
            if (target != null)
            {
                _context.Blogpost.Remove(target);
                await _context.SaveChangesAsync();
                if (System.IO.File.Exists("Res/BlogThumbnails/" + postid + ".png"))
                {
                    System.IO.File.Delete("Res/BlogThumbnails/" + postid + ".png");
                }
                return Ok();
            }
            return NotFound();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> UploadThumbnail()
        {
            var target = await _context.Blogpost.LastOrDefaultAsync();

            if (target == null || Request.Form.Files.Count == 0)
            {
                return BadRequest("Blog list is empty or request file missing");
            }
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "BlogThumbnails");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, target.Postid + ".png");
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest("Empty file");
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> UploadImage()
        {
            try
            {
                //var target = await _context.Blogpost.SingleOrDefaultAsync(bp => bp.Postid == postid);

                //if (target == null)
                //{
                //    return BadRequest("Invalid blogpost");
                //}
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest("No file found in this request");
                }
                //var obj = new Blogpostimages();
                //obj.Blogpost = postid;
                var fileName = Guid.NewGuid().ToString() + ".png";
                //obj.Filename = fileName;
                //_context.Blogpostimages.Add(obj);

                await _context.SaveChangesAsync();
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "BlogImages");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return Ok(Path.Combine(folderName, fileName));
                }
                else
                {
                    return BadRequest("Empty file");
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }

        [Route("[action]/{post}")]
        [HttpPost]
        public async Task<ActionResult> ChangeThumbnail(int post)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(bp => bp.Postid == post);

            if (target == null || Request.Form.Files.Count == 0)
            {
                return BadRequest("Blog post not found or request file missing");
            }
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Res", "BlogThumbnails");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, target.Postid + ".png");
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest("Empty file");
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [Route("[action]/{post}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetThumbnail(int post)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == post);
            if (target == null)
            {
                return NotFound();
            }
            var folderName = Path.Combine("Res", "BlogThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, target.Postid + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return Path.Combine(folderName, target.Postid + ".png");
        }

        [Route("[action]/{post}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetThumbnailRaw(int post)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == post);
            if (target == null)
            {
                return NotFound();
            }
            var folderName = Path.Combine("Res", "BlogThumbnails");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, target.Postid + ".png");
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }
            return await System.IO.File.ReadAllBytesAsync(Path.Combine(folderName, target.Postid + ".png").ToString());
        }

        [Route("[action]/{post}")]
        [HttpGet]
        public async Task<ActionResult<Blogpost[]>> GetRelatedPosts(int post)
        {
            var target = await _context.Blogpost.SingleOrDefaultAsync(obj => obj.Postid == post);

            if (target == null)
            {
                return NotFound();
            }
            List<Blogpost> rposts = new List<Blogpost>();
            foreach (Blogpost bp in _context.Blogpost)
            {
                if (bp == target)
                {
                    continue;
                }
                foreach (string t in target.Tag.Split(";"))
                {
                    if (bp.Tag.Contains(t))
                    {
                        rposts.Add(bp);
                        break;
                    }
                }
            }

            return rposts.ToArray();
        }
    }
}