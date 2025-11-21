using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blogs.App.Features.Tags
{
    public class TagDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class TagDeleteHandler : Service<Tag>, IRequestHandler<TagDeleteRequest, CommandResponse>
    {
        public TagDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(TagDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await Query(false).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Tag not found!");

            Delete(entity);

            return Success("Tag deleted successfully.", entity.Id);
        }
    }
}