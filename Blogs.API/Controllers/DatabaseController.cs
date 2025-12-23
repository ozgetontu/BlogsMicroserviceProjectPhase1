using Blogs.App.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blogs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly BlogsDb _db;
        private readonly IWebHostEnvironment _environment;

        public DatabaseController(BlogsDb db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpGet, Route("~/api/SeedDb")]
        public IActionResult Seed()
        {
            // 1. VERİTABANI OLUŞTURMA
            // Bu satır, 'dotnet ef update' komutuna gerek kalmadan tabloları fiziksel olarak oluşturur.
            _db.Database.EnsureCreated();

            // 2. TEMİZLİK (Eski verileri sil - İlişki sırasına göre)
            // Önce Child (Bağımlı) tablolar, sonra Parent (Ana) tablolar silinmeli.
            if (_db.BlogTags.Any()) _db.BlogTags.RemoveRange(_db.BlogTags.ToList());
            if (_db.Tags.Any()) _db.Tags.RemoveRange(_db.Tags.ToList());
            if (_db.Blogs.Any()) _db.Blogs.RemoveRange(_db.Blogs.ToList());
            if (_db.Users.Any()) _db.Users.RemoveRange(_db.Users.ToList());
            if (_db.Roles.Any()) _db.Roles.RemoveRange(_db.Roles.ToList());

            _db.SaveChanges();

            // ID SAYAÇLARINI SIFIRLA (SQLite'a özel temizlik)
            try
            {
                _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Blogs';");
                _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Tags';");
                _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Users';");
                _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Roles';");
            }
            catch { /* Tablolar boşsa hata vermesin */ }

            // 3. ROLLERİ EKLE
            var adminRole = new Role { Guid = Guid.NewGuid().ToString(), Name = "Admin" };
            var userRole = new Role { Guid = Guid.NewGuid().ToString(), Name = "User" };

            _db.Roles.AddRange(adminRole, userRole);
            _db.SaveChanges(); // Id'ler oluşsun diye hemen kaydediyoruz

            // 4. KULLANICILARI EKLE
            var adminUser = new User
            {
                Guid = Guid.NewGuid().ToString(),
                UserName = "admin",
                Password = "123", // Şifreleme (Hashing) sonraki adımda eklenebilir, şimdilik düz metin.
                IsActive = true,
                RoleId = adminRole.Id
            };

            var standardUser = new User
            {
                Guid = Guid.NewGuid().ToString(),
                UserName = "user",
                Password = "123",
                IsActive = true,
                RoleId = userRole.Id
            };

            _db.Users.AddRange(adminUser, standardUser);
            _db.SaveChanges(); // Kullanıcı Id'leri oluşsun diye kaydediyoruz

            // 5. TAGLARI EKLE
            var tags = new List<Tag>
            {
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Teknoloji" },
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Yazılım" },
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Hayat" },
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Spor" }
            };
            _db.Tags.AddRange(tags);
            _db.SaveChanges();

            // 6. BLOGLARI EKLE (KULLANICILARA BAĞLIYORUZ!)
            // Artık UserId alanlarına yukarıda oluşturduğumuz kullanıcıların Id'lerini veriyoruz.
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = "Aspire Nedir?",
                    Content = "Aspire yeni bir cloud stack...",
                    Rating = 5,
                    PublishDate = DateTime.Now,
                    UserId = adminUser.Id // Yazar: Admin
                },
                new Blog
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = "C# Dersleri",
                    Content = "C# öğrenmek çok zevkli...",
                    Rating = 4.5m,
                    PublishDate = DateTime.Now.AddDays(-2),
                    UserId = standardUser.Id // Yazar: User
                },
                new Blog
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = "Gezi Notlarım",
                    Content = "Bugün Ankara'yı gezdim...",
                    Rating = 3,
                    PublishDate = DateTime.Now.AddDays(-5),
                    UserId = adminUser.Id // Yazar: Admin
                }
            };
            _db.Blogs.AddRange(blogs);
            _db.SaveChanges();

            // 7. İLİŞKİLERİ KUR (Blog <-> Tag)
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[0].Id, TagId = tags[0].Id }); // Aspire -> Teknoloji
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[0].Id, TagId = tags[1].Id }); // Aspire -> Yazılım
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[1].Id, TagId = tags[1].Id }); // C# -> Yazılım
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[2].Id, TagId = tags[2].Id }); // Gezi -> Hayat

            _db.SaveChanges();

            return Ok("Database seed successful. Users, Roles, Blogs and Tags created.");
        }
    }
}