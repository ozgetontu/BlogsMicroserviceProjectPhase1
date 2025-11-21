using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Domain
{
    public class Blog : Entity
    {
        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public decimal? Rating { get; set; }

        public DateTime PublishDate { get; set; }

        // UserId is just an integer here (Microservice architecture rule: weak reference to other service)
        public int UserId { get; set; }

        // Many-to-Many relationship collection
        public List<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
    }
}