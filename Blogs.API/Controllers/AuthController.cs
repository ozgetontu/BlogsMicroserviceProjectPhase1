using Blogs.App.Features.Users;
using CORE.APP.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blogs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/Auth/Login
        // Kullanıcı adı ve şifre ile giriş yapıp Token alma işlemi
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginQueryRequest request)
        {
            var response = await _mediator.Send(request);

            if (response.IsSuccessful)
                return Ok(response); // Başarılıysa 200 OK ve Token döner

            return Unauthorized(response); // Başarısızsa 401 Unauthorized döner
        }

        // POST: api/Auth/Register
        // Yeni kullanıcı kaydetme işlemi
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterCommand request)
        {
            var response = await _mediator.Send(request);

            if (response.IsSuccessful)
                return Ok(response); // Başarılıysa 200 OK döner

            return BadRequest(response); // Başarısızsa 400 Bad Request döner
        }
    }
}