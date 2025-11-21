using CORE.APP.Extensions; // <--- BUNU EKLEMEK ÇOK ÖNEMLİ
using CORE.APP.Models.Ordering;
using CORE.APP.Models.Paging;
using CORE.APP.Services;
using Blogs.App.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Blogs.App.Features.BlogReports
{
    public class BlogReportInnerJoinQueryRequest : IRequest<IQueryable<BlogReportInnerJoinQueryResponse>>,
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

    public class BlogReportInnerJoinQueryResponse
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
    }

    public class BlogReportInnerJoinQueryHandler : Service<Blog>, IRequestHandler<BlogReportInnerJoinQueryRequest, IQueryable<BlogReportInnerJoinQueryResponse>>
    {
        public BlogReportInnerJoinQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<BlogReportInnerJoinQueryResponse>> Handle(BlogReportInnerJoinQueryRequest request, CancellationToken cancellationToken)
        {
            var blogQuery = Query();
            var tagQuery = Query<Tag>();
            var blogTagQuery = Query<BlogTag>();

            var innerJoinQuery = from blog in blogQuery
                                 join bt in blogTagQuery on blog.Id equals bt.BlogId
                                 join tag in tagQuery on bt.TagId equals tag.Id
                                 select new BlogReportInnerJoinQueryResponse
                                 {
                                     BlogId = blog.Id,
                                     BlogTitle = blog.Title,
                                     TagId = tag.Id,
                                     TagName = tag.Name
                                 };

            if (request.OrderEntityPropertyName == "BlogTitle")
            {
                innerJoinQuery = request.IsOrderDescending
                    ? innerJoinQuery.OrderByDescending(x => x.BlogTitle)
                    : innerJoinQuery.OrderBy(x => x.BlogTitle);
            }
            else if (request.OrderEntityPropertyName == "TagName")
            {
                innerJoinQuery = request.IsOrderDescending
                    ? innerJoinQuery.OrderByDescending(x => x.TagName)
                    : innerJoinQuery.OrderBy(x => x.TagName);
            }

            // --- DEĞİŞİKLİK BURADA ---
            // Eski hali: if (!string.IsNullOrWhiteSpace(request.BlogTitle))
            // Yeni hali (Hocanın Extension Metodu):
            if (request.BlogTitle.HasAny())
            {
                innerJoinQuery = innerJoinQuery.Where(x => x.BlogTitle.Contains(request.BlogTitle.Trim()));
            }

            if (request.TagName.HasAny())
            {
                innerJoinQuery = innerJoinQuery.Where(x => x.TagName.Contains(request.TagName.Trim()));
            }
            // -------------------------

            request.TotalCountForPaging = innerJoinQuery.Count();

            if (request.PageNumber > 0 && request.CountPerPage > 0)
            {
                var skipValue = (request.PageNumber - 1) * request.CountPerPage;
                innerJoinQuery = innerJoinQuery.Skip(skipValue).Take(request.CountPerPage);
            }

            return Task.FromResult(innerJoinQuery);
        }
    }
}