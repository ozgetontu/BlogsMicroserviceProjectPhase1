using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Features.Blogs
{
    public class BlogCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(200)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public decimal? Rating { get; set; }
        public DateTime PublishDate { get; set; }
        public int UserId { get; set; }
    }

    public class BlogCreateHandler : Service<Blog>, IRequestHandler<BlogCreateRequest, CommandResponse>
    {
        public BlogCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BlogCreateRequest request, CancellationToken cancellationToken)
        {
            if (await Query().AnyAsync(b => b.Title == request.Title.Trim(), cancellationToken))
                return Error("Blog with the same title exists!");

            var entity = new Blog
            {
                Title = request.Title.Trim(),
                Content = request.Content,
                Rating = request.Rating,
                PublishDate = request.PublishDate,
                UserId = request.UserId
            };

            Create(entity);

            return Success("Blog created successfully.", entity.Id);
        }
    }
}