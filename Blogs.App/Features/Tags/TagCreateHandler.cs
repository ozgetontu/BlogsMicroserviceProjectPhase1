using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Features.Tags
{
    public class TagCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    public class TagCreateHandler : Service<Tag>, IRequestHandler<TagCreateRequest, CommandResponse>
    {
        public TagCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(TagCreateRequest request, CancellationToken cancellationToken)
        {
            // Aynı isimde etiket var mı kontrolü
            if (await Query().AnyAsync(t => t.Name == request.Name.Trim(), cancellationToken))
                return Error("Tag with the same name exists!");

            var entity = new Tag
            {
                Name = request.Name.Trim()
            };

            Create(entity);

            return Success("Tag created successfully.", entity.Id);
        }
    }
}