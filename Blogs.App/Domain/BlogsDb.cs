using Microsoft.EntityFrameworkCore;

namespace Blogs.App.Domain
{
    public class BlogsDb : DbContext
    {
        // Phase 1 Tabloları
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }

        // Phase 2 Tabloları (YENİ EKLENDİ)
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public BlogsDb(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Phase 1: Blog & Tag Ayarları ---

            // Tag ismi benzersiz olsun
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();

            // BlogTag (Çoka-Çok) Anahtar Tanımı
            modelBuilder.Entity<BlogTag>().HasKey(bt => new { bt.BlogId, bt.TagId });

            // BlogTag -> Blog İlişkisi
            modelBuilder.Entity<BlogTag>()
                .HasOne(bt => bt.Blog)
                .WithMany(b => b.BlogTags)
                .HasForeignKey(bt => bt.BlogId)
                .OnDelete(DeleteBehavior.Cascade); // Blog silinirse etiket ilişkisi silinsin

            // BlogTag -> Tag İlişkisi
            modelBuilder.Entity<BlogTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BlogTags)
                .HasForeignKey(bt => bt.TagId)
                .OnDelete(DeleteBehavior.Cascade); // Etiket silinirse blog ilişkisi silinsin

            // --- Phase 2: User & Role Ayarları (YENİ) ---

            // Kullanıcı adı benzersiz olsun
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

            // User -> Role İlişkisi (1 Rol -> Çok Kullanıcı)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- Phase 2: Blog & User BİRLEŞTİRME ---
            // Bir Blog'un bir yazarı (User) vardır.
            // Bir User'ın çok Blog'u vardır.
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.User)
                .WithMany(u => u.Blogs)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Kullanıcı silinirse bloglar kalsın
        }
    }
}