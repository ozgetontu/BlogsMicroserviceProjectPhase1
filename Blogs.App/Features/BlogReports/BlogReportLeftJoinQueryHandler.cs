using CORE.APP.Extensions;
using CORE.APP.Models.Ordering;
using CORE.APP.Models.Paging;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Blogs.App.Features.BlogReports
{
    /// <summary>
    /// Represents a request for a left outer join query between blogs and tags.
    /// </summary>
    public class BlogReportLeftJoinQueryRequest : IRequest<IQueryable<BlogReportLeftJoinQueryResponse>>,
        IPageRequest, IOrderRequest
    {
        public string BlogTitle { get; set; }
        public string TagName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int CountPerPage { get; set; }
        [JsonIgnore]
        public int TotalCountForPaging { get; set; }
        public string OrderEntityPropertyName { get; set; } = "BlogTitle";
        public bool IsOrderDescending { get; set; }
    }

    /// <summary>
    /// Represents the response object for the left outer join query.
    /// </summary>
    public class BlogReportLeftJoinQueryResponse
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }

        // Left join olduğu için Tag gelmeyebilir, nullable (soru işaretli) yapıyoruz.
        public int? TagId { get; set; }
        public string TagName { get; set; }
    }

    public class BlogReportLeftJoinQueryHandler : Service<Blog>, IRequestHandler<BlogReportLeftJoinQueryRequest, IQueryable<BlogReportLeftJoinQueryResponse>>
    {
        public BlogReportLeftJoinQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<BlogReportLeftJoinQueryResponse>> Handle(BlogReportLeftJoinQueryRequest request, CancellationToken cancellationToken)
        {
            var blogQuery = Query();
            var tagQuery = Query<Tag>();
            var blogTagQuery = Query<BlogTag>();

            // Left Join İşlemi (3 Tabloyu birleştiriyoruz)
            // Blog tablosunu al, BlogTag ile eşleştir (eşleşmese de Blog gelsin)
            // Sonra sonucu Tag ile eşleştir.
            var leftJoinQuery = from blog in blogQuery
                                join bt in blogTagQuery on blog.Id equals bt.BlogId into blogTags
                                from bt in blogTags.DefaultIfEmpty()
                                join tag in tagQuery on bt.TagId equals tag.Id into tags
                                from tag in tags.DefaultIfEmpty()
                                select new BlogReportLeftJoinQueryResponse
                                {
                                    BlogId = blog.Id,
                                    BlogTitle = blog.Title,
                                    // Eğer bt (ara tablo) null ise TagId null olsun
                                    TagId = bt != null ? bt.TagId : (int?)null,
                                    // Eğer tag null ise TagName null olsun
                                    TagName = tag != null ? tag.Name : null
                                };

            // Sıralama (Ordering)
            if (request.OrderEntityPropertyName == "BlogTitle")
            {
                leftJoinQuery = request.IsOrderDescending
                    ? leftJoinQuery.OrderByDescending(x => x.BlogTitle)
                    : leftJoinQuery.OrderBy(x => x.BlogTitle);
            }
            else if (request.OrderEntityPropertyName == "TagName")
            {
                leftJoinQuery = request.IsOrderDescending
                    ? leftJoinQuery.OrderByDescending(x => x.TagName)
                    : leftJoinQuery.OrderBy(x => x.TagName);
            }

            // Filtreleme (Filtering)
            // Hata alırsan HasAny yerine !string.IsNullOrWhiteSpace kullan.
            // Hocanın kodundaki gibi null kontrolü (?? "") yaparak filtreliyoruz.
            if (!string.IsNullOrWhiteSpace(request.BlogTitle))
            {
                leftJoinQuery = leftJoinQuery.Where(x => (x.BlogTitle ?? "").Contains(request.BlogTitle.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(request.TagName))
            {
                leftJoinQuery = leftJoinQuery.Where(x => (x.TagName ?? "").Contains(request.TagName.Trim()));
            }

            // Sayfalama (Paging)
            request.TotalCountForPaging = leftJoinQuery.Count();

            if (request.PageNumber > 0 && request.CountPerPage > 0)
            {
                var skipValue = (request.PageNumber - 1) * request.CountPerPage;
                leftJoinQuery = leftJoinQuery.Skip(skipValue).Take(request.CountPerPage);
            }

            return Task.FromResult(leftJoinQuery);
        }
    }
}