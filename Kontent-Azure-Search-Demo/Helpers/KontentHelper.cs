using Kentico.Kontent.Delivery;
using Kontent_Azure_Search_Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kontent_Azure_Search_Demo.Helpers
{
    public class KontentHelper
    {
        public static async Task<ContentItem> GetArticleByUrlPattern(string projectId, string urlPattern)
        {
            var client = DeliveryClientBuilder.WithProjectId(projectId).Build();
            var parameters = new List<IQueryParameter>
            {
                new EqualsFilter("system.type", "article"),
                new EqualsFilter("elements.url_pattern", urlPattern),
                new LimitParameter(1)
            };

            var response = await client.GetItemsAsync(parameters);
            return response.Items.FirstOrDefault();
        }

        public static async Task<IEnumerable<Article>> GetArticlesForSearch(string projectId)
        {
            var client = DeliveryClientBuilder.WithProjectId(projectId).Build();
            var parameters = new List<IQueryParameter>
            {
                new EqualsFilter("system.type", "article"),
                new OrderParameter("elements.post_date", SortOrder.Descending)
            };

            var response = await client.GetItemsAsync(parameters);

            return response.Items.Select(GetArticle);
        }
        
        public static async Task<IEnumerable<Article>> GetArticlesForSearch(string projectId, IEnumerable<string> ids)
        {
            var client = DeliveryClientBuilder.WithProjectId(projectId).Build();
            var parameters = new List<IQueryParameter>
            {
                new EqualsFilter("system.type", "article"),
                new InFilter("system.id", string.Join(',',ids)),
                new LimitParameter(1)
            };

            var response = await client.GetItemsAsync(parameters);
            return response.Items.Select(GetArticle);
        }
        
        private static Article GetArticle(ContentItem item)
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
