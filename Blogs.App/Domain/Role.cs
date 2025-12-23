using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Domain
{
    public class Role : Entity
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        // İlişki: Bir rolün birden çok kullanıcısı olabilir (1-N İlişki)
        public List<User> Users { get; set; } = new List<User>();
    }
}