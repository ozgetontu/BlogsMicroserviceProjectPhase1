using Blogs.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blogs.App.Features.Users
{
    // 1. İSTEK MODELİ (Request): Kullanıcıdan ne istiyoruz?
    public class LoginQueryRequest : IRequest<CommandResponse>
    {
        [Required(ErrorMessage = "User Name is required!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; }
    }

    // 2. İŞ MANTIĞI (Handler): Giriş kontrolü ve Token üretimi
    public class LoginQueryHandler : Service<User>, IRequestHandler<LoginQueryRequest, CommandResponse>
    {
        private readonly IConfiguration _configuration;

        // Constructor: Veritabanı ve Ayar Dosyasına (appsettings.json) erişim
        public LoginQueryHandler(DbContext db, IConfiguration configuration) : base(db)
        {
            _configuration = configuration;
        }

        public async Task<CommandResponse> Handle(LoginQueryRequest request, CancellationToken cancellationToken)
        {
            // Kullanıcıyı bul (Rolüyle birlikte)
            var user = await Query().Include(u => u.Role)
                                    .SingleOrDefaultAsync(u => u.UserName == request.UserName && u.Password == request.Password, cancellationToken);

            // Kullanıcı yoksa veya şifre yanlışsa hata dön
            if (user is null)
                return Error("Invalid user name or password!");

            // Kullanıcı pasifse hata dön
            if (!user.IsActive)
                return Error("User is not active!");

            // Her şey doğruysa Token üret
            string token = CreateJwtToken(user);

            // Başarılı cevabın mesaj kısmına Token'ı koyup dönüyoruz.
            // (Gerçek projelerde ayrı bir DTO dönmek daha iyidir ama hocanın yapısına uygun olarak böyle yapıyoruz)
            return new CommandResponse(true, token, user.Id);
        }

        // --- JWT TOKEN ÜRETME METODU ---
        private string CreateJwtToken(User user)
        {
            // Gizli anahtarı appsettings.json'dan al
            var key = _configuration["SecurityKey"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Token içine gömülecek bilgiler (Claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name) // Rol bilgisi (Admin/User) burada saklanır!
            };

            // Token ayarlarını yap (Süre, yayıncı, dinleyici)
            var token = new JwtSecurityToken(
                issuer: _configuration["Issuer"],
                audience: _configuration["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24), // Token 24 saat geçerli olsun
                signingCredentials: credentials);

            // Token'ı string olarak oluştur
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}