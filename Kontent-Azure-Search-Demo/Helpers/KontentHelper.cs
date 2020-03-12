using Kentico.Kontent.Delivery;
using Kontent_Azure_Search_Demo.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent_Azure_Search_Demo.Helpers
{
    public class KontentHelper
    {
        private readonly IDeliveryClient deliveryClient;

        public KontentHelper(IConfiguration configuration)
        {
            var projectId = configuration["KontentProjectID"];
            this.deliveryClient = DeliveryClientBuilder.WithProjectId(projectId).Build();
        }

        public async Task<ContentItem> GetArticleByUrlPattern(string urlPattern)
        {
            var parameters = new List<IQueryParameter>
            {
                new EqualsFilter("system.type", "article"),
                new EqualsFilter("elements.url_pattern", urlPattern),
                new LimitParameter(1)
            };

            var response = await deliveryClient.GetItemsAsync(parameters);
            return response.Items.FirstOrDefault();
        }

        public async Task<IEnumerable<Article>> GetArticlesForSearch()
        {
            var parameters = new List<IQueryParameter>
            {
                new EqualsFilter("system.type", "article"),
                new OrderParameter("elements.post_date", SortOrder.Descending)
            };

            var response = await deliveryClient.GetItemsAsync(parameters);

            return response.Items.Select(GetArticle);
        }

        public async Task<IEnumerable<Article>> GetArticlesForSearch(IEnumerable<string> ids)
        {
            var parameters = new List<IQueryParameter>
            {
                new EqualsFilter("system.type", "article"),
                new InFilter("system.id", string.Join(',',ids)),
                new LimitParameter(1)
            };

            var response = await deliveryClient.GetItemsAsync(parameters);
            return response.Items.Select(GetArticle);
        }

        private Article GetArticle(ContentItem item)
        {
            if (item == null)
            {
                return new Article();
            }

            return new Article
            {
                Body = item.GetString("body_copy"),
                ID = item.System.Id,
                PostDate = item.GetDateTime("post_date"),
                Summary = item.GetString("summary"),
                Title = item.GetString("title"),
                UrlPattern = item.GetString("url_pattern"),
            };
        }
    }
}
