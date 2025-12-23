using Blogs.App.Features.BlogReports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogs.API.Controllers
{
    /// <summary>
    /// API controller for handling blog-tag join operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // KİLİT: Bu Controller'daki metodlara sadece giriş yapmış (Authenticated) kullanıcılar erişebilir.
    public class BlogReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BlogReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> InnerJoin(BlogReportInnerJoinQueryRequest request)
        {
            // Send the request to the MediatR pipeline
            var response = await _mediator.Send(request);

            // Materialize the queryable result to a list asynchronously.
            var list = await response.ToListAsync();

            // If the result contains any records, return them with HTTP 200 OK.
            if (list.Any())
                return Ok(list);

            // If no records are found, return HTTP 204 No Content.
            return NoContent();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LeftJoin(BlogReportLeftJoinQueryRequest request)
        {
            // Send the request to the MediatR pipeline
            var response = await _mediator.Send(request);

            // Materialize the queryable result to a list asynchronously.
            var list = await response.ToListAsync();

            // If the result contains any records, return them with HTTP 200 OK.
            if (list.Any())
                return Ok(list);

            // If no records are found, return HTTP 204 No Content.
            return NoContent();
        }
    }
}