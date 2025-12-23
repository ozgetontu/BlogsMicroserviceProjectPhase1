using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Domain
{
    public class User : Entity
    {
        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(20)]
        public string Password { get; set; }

        public bool IsActive { get; set; }

        // Role İlişkisi (Zorunlu - Her kullanıcının bir rolü vardır)
        public int RoleId { get; set; }
        public Role Role { get; set; }

        // Blog İlişkisi (Kritik Nokta: İki diyagramı birleştiriyoruz)
        // Bir kullanıcının birden çok blog yazısı olabilir.
        public List<Blog> Blogs { get; set; } = new List<Blog>();
    }
}