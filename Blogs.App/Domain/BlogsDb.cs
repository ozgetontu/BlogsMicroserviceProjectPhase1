using Microsoft.EntityFrameworkCore;

namespace Blogs.App.Domain
{
    public class BlogsDb : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }

        public BlogsDb(DbContextOptions options) : base(options)
        {
        }

        // Overriding OnModelCreating method is optional.
        /// <summary>
        /// Configures the entity relationships and database schema rules for the application domain.
        /// This method defines how entities are related, sets up foreign key constraints, and customizes
        /// the delete behavior for each relationship to prevent cascading deletes.
        /// </summary>
        /// <param name="modelBuilder">
        /// The <see cref="ModelBuilder"/> used to configure entity mappings and relationships.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Index configurations:
            // Defining unique indices to enforce uniqueness constraints on certain properties.

            // Name data of the Tags table can not have multiple same values.
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();

            // Many-to-Many Configuration (BlogTag):
            // Define composite primary key for the join table.
            modelBuilder.Entity<BlogTag>().HasKey(bt => new { bt.BlogId, bt.TagId });

            // Relationship configurations:
            // Configuration should start with the entities that have the foreign keys.

            // BlogTag -> Blog Relationship
            modelBuilder.Entity<BlogTag>()
                .HasOne(bt => bt.Blog)
                .WithMany(b => b.BlogTags)
                .HasForeignKey(bt => bt.BlogId)
                .OnDelete(DeleteBehavior.Cascade); // If a Blog is deleted, its tags links should be deleted.

            // BlogTag -> Tag Relationship
            modelBuilder.Entity<BlogTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BlogTags)
                .HasForeignKey(bt => bt.TagId)
                .OnDelete(DeleteBehavior.Cascade); // If a Tag is deleted, its links to blogs should be deleted.
        }
    }
}