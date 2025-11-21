using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blogs.App.Features.Blogs
{
    public class BlogDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class BlogDeleteHandler : Service<Blog>, IRequestHandler<BlogDeleteRequest, CommandResponse>
    {
        public BlogDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BlogDeleteRequest request, CancellationToken cancellationToken)
        {
            // Blog silinince ilişkili Tag bağlantılarını (BlogTag tablosu) da temizlemek gerekir
            // Ancak veritabanında Cascade Delete tanımladığımız için EF Core bunu otomatik yapabilir.
            var entity = await Query(false).SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Blog not found!");

            Delete(entity);

            return Success("Blog deleted successfully.", entity.Id);
        }
    }
}