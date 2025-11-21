using CORE.APP.Models;
using CORE.APP.Services;
using Blogs.App.Domain;
using Blogs.App.Features.Tags;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization; // Bu kütüphane JSON ayarları için gerekli

namespace Blogs.App.Features.Blogs
{
    public class BlogQueryRequest : Request, IRequest<IQueryable<BlogQueryResponse>>
    {
    }

    public class BlogQueryResponse : Response
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public decimal? Rating { get; set; }

        // --- TARİH AYARI ---

        // 1. Veritabanından gelen ham tarih (Bunu gizliyoruz)
        [JsonIgnore]
        public DateTime RawPublishDate { get; set; }

        // 2. Kullanıcının göreceği formatlı tarih
        // JSON çıktısında ismi "publishDate" olsun istiyoruz.
        [JsonPropertyName("publishDate")]
        public string PublishDate => RawPublishDate.ToString("dd.MM.yyyy HH:mm");

        // -------------------

        public List<TagQueryResponse> Tags { get; set; }
    }

    public class BlogQueryHandler : Service<Blog>, IRequestHandler<BlogQueryRequest, IQueryable<BlogQueryResponse>>
    {
        protected override IQueryable<Blog> Query(bool isNoTracking = true)
        {
            return base.Query(isNoTracking)
                       .Include(b => b.BlogTags)
                       .ThenInclude(bt => bt.Tag)
                       .OrderByDescending(b => b.PublishDate);
        }

        public BlogQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<BlogQueryResponse>> Handle(BlogQueryRequest request, CancellationToken cancellationToken)
        {
            var query = Query().Select(b => new BlogQueryResponse
            {
                Id = b.Id,
                Guid = b.Guid,
                Title = b.Title,
                Content = b.Content,
                Rating = b.Rating,

                // Veritabanındaki tarihi, bizim gizli (Raw) özelliğimize atıyoruz.
                // 'PublishDate' özelliği (string olan) otomatik olarak buradan okuyup formatlayacak.
                RawPublishDate = b.PublishDate,

                Tags = b.BlogTags.Select(bt => new TagQueryResponse
                {
                    Id = bt.Tag.Id,
                    Guid = bt.Tag.Guid,
                    Name = bt.Tag.Name
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}