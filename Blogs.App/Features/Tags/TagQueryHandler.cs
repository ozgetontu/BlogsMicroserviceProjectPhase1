using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blogs.App.Features.Tags
{
    public class TagQueryRequest : Request, IRequest<IQueryable<TagQueryResponse>>
    {
        // Filtreleme gerekirse buraya eklenir
    }

    public class TagQueryResponse : Response
    {
        public string Name { get; set; }
    }

    public class TagQueryHandler : Service<Tag>, IRequestHandler<TagQueryRequest, IQueryable<TagQueryResponse>>
    {
        protected override IQueryable<Tag> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking).OrderBy(t => t.Name);
        }

        public TagQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<TagQueryResponse>> Handle(TagQueryRequest request, CancellationToken cancellationToken)
        {
            var query = Query().Select(t => new TagQueryResponse
            {
                Id = t.Id,
                Guid = t.Guid,
                Name = t.Name
            });

            return Task.FromResult(query);
        }
    }
}