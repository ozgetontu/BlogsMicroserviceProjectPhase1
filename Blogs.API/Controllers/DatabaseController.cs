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
            // 1. TABLOLARI OLUŞTUR (Eğer yoksa)
            // Bu satır, 'dotnet ef update' komutuna gerek kalmadan tabloları kurar.
            _db.Database.EnsureCreated();

            // 2. ESKİ VERİLERİ TEMİZLE
            if (_db.BlogTags.Any()) _db.BlogTags.RemoveRange(_db.BlogTags.ToList());
            if (_db.Tags.Any()) _db.Tags.RemoveRange(_db.Tags.ToList());
            if (_db.Blogs.Any()) _db.Blogs.RemoveRange(_db.Blogs.ToList());

            _db.SaveChanges();

            // 3. ID SAYAÇLARINI SIFIRLA (SQLite için)
            try
            {
                _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Blogs';");
                _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Tags';");
            }
            catch { /* Tablo boşsa hata vermesin */ }

            // 4. TAGLARI EKLE (GUID İLE)
            var tags = new List<Tag>
            {
                // Guid alanı Entity'de zorunlu olduğu için burada elle veriyoruz.
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Teknoloji" },
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Yazılım" },
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Hayat" },
                new Tag { Guid = Guid.NewGuid().ToString(), Name = "Spor" }
            };
            _db.Tags.AddRange(tags);
            _db.SaveChanges();

            // 5. BLOGLARI EKLE (GUID İLE)
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = "Aspire Nedir?",
                    Content = "Aspire yeni bir cloud stack...",
                    Rating = 5,
                    PublishDate = DateTime.Now,
                    UserId = 1
                },
                new Blog
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = "C# Dersleri",
                    Content = "C# öğrenmek çok zevkli...",
                    Rating = 4.5m,
                    PublishDate = DateTime.Now.AddDays(-2),
                    UserId = 1
                },
                new Blog
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = "Gezi Notlarım",
                    Content = "Bugün Ankara'yı gezdim...",
                    Rating = 3,
                    PublishDate = DateTime.Now.AddDays(-5),
                    UserId = 2
                }
            };
            _db.Blogs.AddRange(blogs);
            _db.SaveChanges();

            // 6. İLİŞKİLERİ KUR (Blog <-> Tag)
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[0].Id, TagId = tags[0].Id }); // Aspire -> Teknoloji
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[0].Id, TagId = tags[1].Id }); // Aspire -> Yazılım
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[1].Id, TagId = tags[1].Id }); // C# -> Yazılım
            _db.BlogTags.Add(new BlogTag { BlogId = blogs[2].Id, TagId = tags[2].Id }); // Gezi -> Hayat

            _db.SaveChanges();

            return Ok("Database seed successful. Tables created and data inserted.");
        }
    }
}