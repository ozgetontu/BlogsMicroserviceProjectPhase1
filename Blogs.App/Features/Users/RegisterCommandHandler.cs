using Blogs.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Features.Users
{
    // 1. İSTEK MODELİ (Request): Kayıt için gerekenler
    public class RegisterCommand : IRequest<CommandResponse>
    {
        [Required(ErrorMessage = "User Name is required!")]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [StringLength(20)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required!")]
        [Compare("Password", ErrorMessage = "Passwords do not match!")] // Şifre tekrarı kontrolü
        public string ConfirmPassword { get; set; }
    }

    // 2. İŞ MANTIĞI (Handler): Kayıt işlemi
    public class RegisterCommandHandler : Service<User>, IRequestHandler<RegisterCommand, CommandResponse>
    {
        public RegisterCommandHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Validasyon 1: Aynı kullanıcı adı var mı?
            // Trim() ile boşlukları temizliyoruz.
            if (await Query().AnyAsync(u => u.UserName == request.UserName.Trim(), cancellationToken))
                return Error("User name already exists!");

            // Validasyon 2: Varsayılan "User" rolünü bul
            // Service sınıfındaki generic Query metodunu kullanarak Role tablosuna erişiyoruz.
            var userRole = await Query<Role>().FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);

            if (userRole == null)
                return Error("Default 'User' role not found! Please run SeedDb first.");

            // Yeni kullanıcıyı oluştur
            var newUser = new User
            {
                UserName = request.UserName.Trim(),
                Password = request.Password, // Phase 2 isterlerine göre şifreleme zorunlu değil, düz metin kaydediyoruz.
                IsActive = true, // Yeni kayıt olan kullanıcı aktif olsun
                RoleId = userRole.Id
            };

            // Veritabanına ekle
            Create(newUser);

            return Success("User registered successfully.", newUser.Id);
        }
    }
}