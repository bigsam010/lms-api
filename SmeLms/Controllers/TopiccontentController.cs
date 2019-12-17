using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmeLms.Models;
namespace SmeLms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopiccontentController : ControllerBase
    {
        smelmsContext _context;
        public TopiccontentController(smelmsContext _context)
        {
            this._context = _context;
        }

        [HttpGet]
        public async Task<ActionResult<Topiccontent[]>> Get()
        {
            return await _context.Topiccontent.ToArrayAsync();
        }

        [HttpGet("{contentid}")]
        public async Task<ActionResult<Topiccontent>> Get(int contentid)
        {
            var target = await _context.Topiccontent.SingleOrDefaultAsync(obj => obj.Contentid == contentid);
            if (target == null)
            {
                return NotFound();
            }
            return target;
        }

        [Route("[action]/{topicid}")]
        [HttpGet]
        public async Task<ActionResult<Topiccontent[]>> GetTopicContent(int topicid)
        {
            return await _context.Topiccontent.Where(obj => obj.Topicid == topicid).OrderBy(obj => obj.Contentposition).ToArrayAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Topiccontent>> Post([FromBody] Topiccontent obj)
        {
            List<string> formats = new List<string> {
                "xls","xlsx","csv","pptx","ppt","pdf","png","jpg","docx","doc","mp4"
            };
            if (!ModelState.IsValid || PosExists(obj.Topicid, obj.Contentposition))
            {
                return BadRequest("Invalid content position. Position already assigned");
            }
            else if (obj.Contenttype.ToLower() == "file" && !formats.Contains(obj.Fileformat.ToLower()))
            {
                return BadRequest("Invalid file format");
            }
            else
            {
                _context.Topiccontent.Add(obj);
                await _context.SaveChangesAsync();
                return Created("api/Topiccontent", obj);
            }
        }

        [HttpPut("{contentid}")]
        public async Task<ActionResult> Put(int contentid, [FromBody] Topiccontent obj)
        {
            var target = await _context.Topiccontent.SingleOrDefaultAsync(nobj => nobj.Contentid == contentid);

            if (target != null && ModelState.IsValid)
            {
                _context.Entry(target).CurrentValues.SetValues(obj);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{contentid}")]
        public async Task<ActionResult> Delete(int contentid)
        {
            var target = await _context.Topiccontent.SingleOrDefaultAsync(obj => obj.Contentid == contentid);
            if (target != null)
            {
                _context.Topiccontent.Remove(target);
                await _context.SaveChangesAsync();
                if (target.Contenttype.ToLower() == "file")
                {
                    var folderName = Path.Combine("Res", "Attachments");
                    var attach = Path.Combine(folderName, "sme_att_" + target.Contentid + "_" + target.Content);
                    if (System.IO.File.Exists(attach))
                    {
                        System.IO.File.Delete(attach);
                    }
                }
                return Ok();
            }
            return NotFound();
        }
        private bool PosExists(int topicid, int pos)
        {
            var target = _context.Topiccontent.SingleOrDefault(obj => obj.Topicid == topicid && obj.Contentposition == pos);
            return target != null;

        }

        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> UploadAttachment()
        {
            var target = await _context.Topiccontent.LastOrDefaultAsync();
            var folderName = Path.Combine("Res", "Attachments");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (target == null || target.Contenttype.ToLower() != "file" || System.IO.File.Exists(Path.Combine(pathToSave, "sme_att_" + target.Contentid + "_" + target.Content).ToString()))
            {
                return BadRequest("Invalid model state or invalid content of file type");
            }
            try
            {
                var file = Request.Form.Files[0];

                if (file.Length > 0)
                {
                    var fullPath = Path.Combine(pathToSave, "sme_att_" + target.Contentid + "_" + target.Content);
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

        [Route("[action]/{contentid}")]
        [HttpGet]
        public async Task<ActionResult<string>> GetAttachment(int contentid)
        {
            var target = await _context.Topiccontent.SingleOrDefaultAsync(obj => obj.Contentid == contentid);
            if (target == null || target.Contenttype.ToLower() != "file")
            {
                return BadRequest();
            }
            var folderName = Path.Combine("Res", "Attachments");
            if (!System.IO.File.Exists(Path.Combine(folderName, "sme_att_" + target.Contentid + "_" + target.Content).ToString()))
            {
                return NotFound();
            }

            return Path.Combine(folderName, "sme_att_" + target.Contentid + "_" + target.Content);
        }

        [Route("[action]/{contentid}")]
        [HttpGet]
        public async Task<ActionResult<byte[]>> GetAttachmentRaw(int contentid)
        {
            var target = await _context.Topiccontent.SingleOrDefaultAsync(obj => obj.Contentid == contentid);
            if (target == null || target.Contenttype.ToLower() != "file")
            {
                return BadRequest();
            }
            var folderName = Path.Combine("Res", "Attachments");
            if (!System.IO.File.Exists(Path.Combine(folderName, "sme_att_" + target.Contentid + "_" + target.Content).ToString()))
            {
                return NotFound();
            }

            return await System.IO.File.ReadAllBytesAsync(Path.Combine(folderName, "sme_att_" + target.Contentid + "_" + target.Content));
        }
    }
}