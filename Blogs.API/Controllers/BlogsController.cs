#nullable disable
using CORE.APP.Models;
using Blogs.App.Features.Blogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//Generated from Custom Microservices Template.
namespace Blogs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. GENEL KİLİT: Buradaki her şey varsayılan olarak giriş gerektirir.
    public class BlogsController : ControllerBase
    {
        private readonly ILogger<BlogsController> _logger;
        private readonly IMediator _mediator;

        public BlogsController(ILogger<BlogsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // GET: api/Blogs
        [HttpGet]
        [AllowAnonymous] // 2. İSTİSNA: Giriş yapmayanlar da blogları okuyabilir.
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _mediator.Send(new BlogQueryRequest());
                var list = await response.ToListAsync();
                if (list.Any())
                    return Ok(list);
                return NoContent();
            }
            catch (Exception exception)
            {
                _logger.LogError("BlogsGet Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during BlogsGet."));
            }
        }

        // GET: api/Blogs/5
        [HttpGet("{id}")]
        [AllowAnonymous] // 2. İSTİSNA: Tekil okuma da herkese açık.
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var response = await _mediator.Send(new BlogQueryRequest());
                var item = await response.SingleOrDefaultAsync(r => r.Id == id);
                if (item is not null)
                    return Ok(item);

                return NotFound(new CommandResponse(false, "There is no Blog with this id in database!"));
            }
            catch (Exception exception)
            {
                _logger.LogError("BlogsGetById Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during BlogsGetById."));
            }
        }

        // POST: api/Blogs
        [HttpPost]
        [Authorize(Roles = "Admin")] // 3. ÖZEL KİLİT: Sadece "Admin" rolündekiler ekleme yapabilir.
        public async Task<IActionResult> Post(BlogCreateRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _mediator.Send(request);
                    if (response.IsSuccessful)
                    {
                        return Ok(response);
                    }
                    ModelState.AddModelError("BlogsPost", response.Message);
                }
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                _logger.LogError("BlogsPost Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during BlogsPost."));
            }
        }

        // PUT: api/Blogs
        [HttpPut]
        [Authorize(Roles = "Admin")] // 3. ÖZEL KİLİT: Sadece "Admin" güncelleyebilir.
        public async Task<IActionResult> Put(BlogUpdateRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _mediator.Send(request);
                    if (response.IsSuccessful)
                    {
                        return Ok(response);
                    }
                    ModelState.AddModelError("BlogsPut", response.Message);
                }
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                _logger.LogError("BlogsPut Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during BlogsPut."));
            }
        }

        // DELETE: api/Blogs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // 3. ÖZEL KİLİT: Sadece "Admin" silebilir.
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _mediator.Send(new BlogDeleteRequest() { Id = id });
                if (response.IsSuccessful)
                {
                    return Ok(response);
                }
                ModelState.AddModelError("BlogsDelete", response.Message);
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                _logger.LogError("BlogsDelete Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during BlogsDelete."));
            }
        }
    }
}