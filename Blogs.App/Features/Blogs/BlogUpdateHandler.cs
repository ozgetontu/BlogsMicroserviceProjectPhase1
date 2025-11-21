using CORE.APP.Extensions; // Extension metodları için gerekli
using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Features.Blogs
{
    public class BlogUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public decimal? Rating { get; set; }
    }

    public class BlogUpdateHandler : Service<Blog>, IRequestHandler<BlogUpdateRequest, CommandResponse>
    {
        public BlogUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BlogUpdateRequest request, CancellationToken cancellationToken)
        {
            // 1. VALIDATION (BOŞLUK KONTROLÜ)
            if (request.Title.HasNotAny())
                return Error("Title cannot be empty!");

            if (request.Content.HasNotAny())
                return Error("Content cannot be empty!");

            // 2. AYNI BAŞLIK VAR MI KONTROLÜ (KENDİSİ HARİÇ)
            if (await Query().AnyAsync(b => b.Id != request.Id && b.Title == request.Title.Trim(), cancellationToken))
                return Error("Blog with the same title exists!");

            // 3. KAYIT VAR MI KONTROLÜ
            var entity = await Query(false).SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Blog not found!");

            // 4. GÜNCELLEME
            entity.Title = request.Title.Trim();
            entity.Content = request.Content; // Content HTML içerebileceği için Trim yapmadık ama istersen yapabilirsin.
            entity.Rating = request.Rating;

            Update(entity);

            return Success("Blog updated successfully.", entity.Id);
        }
    }
}