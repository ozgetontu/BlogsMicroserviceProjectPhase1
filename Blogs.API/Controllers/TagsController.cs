#nullable disable
using CORE.APP.Models;
using Blogs.App.Features.Tags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // 1. SIKI KİLİT: Bu Controller varsayılan olarak sadece Admin'e açıktır.
    public class TagsController : ControllerBase
    {
        private readonly ILogger<TagsController> _logger;
        private readonly IMediator _mediator;

        public TagsController(ILogger<TagsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // GET: api/Tags
        [HttpGet]
        [AllowAnonymous] // 2. İSTİSNA: Ama okumayı herkes yapabilir.
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _mediator.Send(new TagQueryRequest());
                var list = await response.ToListAsync();
                if (list.Any())
                    return Ok(list);
                return NoContent();
            }
            catch (Exception exception)
            {
                _logger.LogError("TagsGet Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsGet."));
            }
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        [AllowAnonymous] // 2. İSTİSNA: Tekil okumayı da herkes yapabilir.
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var response = await _mediator.Send(new TagQueryRequest());
                var item = await response.SingleOrDefaultAsync(r => r.Id == id);
                if (item is not null)
                    return Ok(item);

                return NotFound(new CommandResponse(false, "There is no tag with this id in the database!"));
            }
            catch (Exception exception)
            {
                _logger.LogError("TagsGetById Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsGetById."));
            }
        }

        // POST: api/Tags
        [HttpPost]
        // Sınıf seviyesindeki [Authorize(Roles = "Admin")] burası için de geçerli.
        public async Task<IActionResult> Post(TagCreateRequest request)
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
                    ModelState.AddModelError("TagsPost", response.Message);
                }
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                _logger.LogError("TagsPost Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsPost."));
            }
        }

        // PUT: api/Tags
        [HttpPut]
        // Sınıf seviyesindeki [Authorize(Roles = "Admin")] burası için de geçerli.
        public async Task<IActionResult> Put(TagUpdateRequest request)
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
                    ModelState.AddModelError("TagsPut", response.Message);
                }
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                _logger.LogError("TagsPut Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsPut."));
            }
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        // Sınıf seviyesindeki [Authorize(Roles = "Admin")] burası için de geçerli.
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _mediator.Send(new TagDeleteRequest() { Id = id });
                if (response.IsSuccessful)
                {
                    return Ok(response);
                }
                ModelState.AddModelError("TagsDelete", response.Message);
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                _logger.LogError("TagsDelete Exception: " + exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsDelete."));
            }
        }
    }
}