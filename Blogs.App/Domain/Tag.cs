using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Domain
{
    public class Tag : Entity
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        public List<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
    }
}