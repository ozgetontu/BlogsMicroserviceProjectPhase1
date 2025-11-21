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
    // [Authorize(Roles = "Admin")] // Security disabled for now
    public class TagsController : ControllerBase
    {
        private readonly ILogger<TagsController> _logger;
        private readonly IMediator _mediator;

        // Constructor: injects logger to log the errors to Kestrel Console or Output Window and mediator
        public TagsController(ILogger<TagsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // GET: api/Tags
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Send a query request to get query response
                var response = await _mediator.Send(new TagQueryRequest());
                // Convert the query response to a list
                var list = await response.ToListAsync();
                // If there are items, return them with 200 OK
                if (list.Any())
                    return Ok(list);
                // If no items found, return 204 No Content
                return NoContent();
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("TagsGet Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsGet."));
            }
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Send a query request to get query response
                var response = await _mediator.Send(new TagQueryRequest());
                // Find the item with the given id
                var item = await response.SingleOrDefaultAsync(r => r.Id == id);
                // If item found, return it with 200 OK
                if (item is not null)
                    return Ok(item);

                // Tag bulunamazsa:
                return NotFound(new CommandResponse(false, "There is no tag with this id in the database!"));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("TagsGetById Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsGetById."));
            }
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<IActionResult> Post(TagCreateRequest request)
        {
            try
            {
                // Check if the request model is valid through data annotations
                if (ModelState.IsValid)
                {
                    // Send the create request
                    var response = await _mediator.Send(request);
                    // If creation is successful, return 200 OK with success command response
                    if (response.IsSuccessful)
                    {
                        //return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
                        return Ok(response);
                    }
                    // If creation failed, add error command response message to model state
                    ModelState.AddModelError("TagsPost", response.Message);
                }
                // Return 400 Bad Request with all data annotation validation error messages and the error command response message if added seperated by |
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("TagsPost Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsPost."));
            }
        }

        // PUT: api/Tags
        [HttpPut]
        public async Task<IActionResult> Put(TagUpdateRequest request)
        {
            try
            {
                // Check if the request model is valid through data annotations
                if (ModelState.IsValid)
                {
                    // Send the update request
                    var response = await _mediator.Send(request);
                    // If update is successful, return 200 OK with success command response
                    if (response.IsSuccessful)
                    {
                        //return NoContent();
                        return Ok(response);
                    }
                    // If update failed, add error command response message to model state
                    ModelState.AddModelError("TagsPut", response.Message);
                }
                // Return 400 Bad Request with all data annotation validation error messages and the error command response message if added seperated by |
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("TagsPut Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsPut."));
            }
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Send the delete request
                var response = await _mediator.Send(new TagDeleteRequest() { Id = id });
                // If delete is successful, return 200 OK with success command response
                if (response.IsSuccessful)
                {
                    //return NoContent();
                    return Ok(response);
                }
                // If delete failed, add error command response message to model state
                ModelState.AddModelError("TagsDelete", response.Message);
                // Return 400 Bad Request with the error command response message
                return BadRequest(new CommandResponse(false, string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
            }
            catch (Exception exception)
            {
                // Log the exception
                _logger.LogError("TagsDelete Exception: " + exception.Message);
                // Return 500 Internal Server Error with an error command response with message
                return StatusCode(StatusCodes.Status500InternalServerError, new CommandResponse(false, "An exception occured during TagsDelete."));
            }
        }
    }
}