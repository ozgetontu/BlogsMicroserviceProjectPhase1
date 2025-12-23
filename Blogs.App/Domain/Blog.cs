using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Domain
{
    public class Blog : Entity
    {
        public Blog()
        {
            // Yeni blog oluştuğunda Guid otomatik atansın
            Guid = System.Guid.NewGuid().ToString();
        }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public decimal? Rating { get; set; }

        public DateTime PublishDate { get; set; }

        // --- YENİ EKLENEN KISIM: User İlişkisi ---
        // UserId: Hangi kullanıcı yazdı?
        public int UserId { get; set; }

        // User: Kod içinde blog.User diyerek yazarın detaylarına (UserName vb.) erişmek için.
        public User User { get; set; }
        // -----------------------------------------

        // Tag İlişkisi (Phase 1'den devam)
        public List<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
    }
}