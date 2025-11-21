using CORE.APP.Extensions; // Extension metodları için gerekli
using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blogs.App.Features.Tags
{
    public class TagUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    public class TagUpdateHandler : Service<Tag>, IRequestHandler<TagUpdateRequest, CommandResponse>
    {
        public TagUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(TagUpdateRequest request, CancellationToken cancellationToken)
        {
            // 1. VALIDATION (BOŞLUK KONTROLÜ)
            // Name alanı null, boş veya sadece boşluktan oluşuyorsa hata dön.
            if (request.Name.HasNotAny())
                return Error("Tag name cannot be empty!");

            // 2. AYNI İSİM VAR MI KONTROLÜ (KENDİSİ HARİÇ)
            if (await Query().AnyAsync(t => t.Id != request.Id && t.Name == request.Name.Trim(), cancellationToken))
                return Error("Tag with the same name exists!");

            // 3. KAYIT VAR MI KONTROLÜ
            // Tracking açık (false) çünkü güncelleme yapacağız.
            var entity = await Query(false).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Tag not found!");

            // 4. GÜNCELLEME
            entity.Name = request.Name.Trim();

            Update(entity);

            return Success("Tag updated successfully.", entity.Id);
        }
    }
}