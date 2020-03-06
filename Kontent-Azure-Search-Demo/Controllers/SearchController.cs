using Kontent_Azure_Search_Demo.Helpers;
using Kontent_Azure_Search_Demo.KontentWebhookModels;
using Kontent_Azure_Search_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent_Azure_Search_Demo.Controllers
{
    public class SearchController : Controller
    {
        private readonly string indexName;
        private readonly string projectId;
        private readonly string webhookSecret;
        private ISearchIndexClient indexClient;
        private SearchServiceClient searchServiceClient;

        public SearchController(IConfiguration configuration)
        {
            projectId = configuration["KontentProjectID"];
            indexName = configuration["SearchIndexName"];
            webhookSecret = configuration["KontentWebhookSecret"];
            searchServiceClient = SearchHelper.CreateSearchServiceClient(configuration);
            indexClient = searchServiceClient.Indexes.GetClient(indexName);
        }

        public IActionResult Create()
        {
            SearchHelper.DeleteIndexIfExists(indexName, searchServiceClient);
            SearchHelper.CreateIndex<Article>(indexName, searchServiceClient);

            ViewData["Message"] = $"{indexName} deleted/created.";
            return View("Index");
        }

        public IActionResult Index(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) {
                return View();
            }

            var indexClient = searchServiceClient.Indexes.GetClient(indexName);
            var results = SearchHelper.QueryIndex<Article>(indexClient, searchText);

            return View(results);
        }

        public IActionResult Initialize()
        {
            var articles = KontentHelper.GetArticlesForSearch(projectId).Result;
            SearchHelper.AddToIndex<Article>(indexClient, articles);

            ViewData["Message"] = $"{indexName} initialized.";
            return View("Index");
        }
        
        [HttpPost("webhook")]
        public async Task<string> ProcessWebhook()
        {
            // Manually read request body so we can use it to validate the request. Body is gone if we use [FromBody] on a parameter.
            string bodyRaw;
            using (var reader = new StreamReader(Request.Body))
            {
                bodyRaw = await reader.ReadToEndAsync();
            }

            var isValid = KontentWebhookHelper.ValidateWebhook(Request.Headers["X-KC-Signature"], bodyRaw, webhookSecret);
            if (!isValid)
            {
                return "Invalid signature";
            }

            // Manually deserialize request body because we needed to use the raw value previously to validate the request
            var webhook = JsonConvert.DeserializeObject<Webhook>(bodyRaw);

            var isContentItemVariantEvent = webhook.Message.Type == "content_item_variant";
            if (!isContentItemVariantEvent)
            {
                return "Not content item variant event";
            }

            var isPublishevent = webhook.Message.Operation == "publish";
            var isUnpublishEvent = webhook.Message.Operation == "unpublish";

            if (!isPublishevent && !isUnpublishEvent)
            {
                return "Not publish/unpublish event";
            }

            var effectedArticles = webhook.Data.Items.Where(i => i.Type == "article" && i.Language == "en-US");
            if (effectedArticles.Count() == 0)
            {
                return "No 'en-US' articles effected by this event";
            }

            if (isPublishevent)
            {
                var articlesToUpdate = KontentHelper.GetArticlesForSearch(projectId, effectedArticles.Select(a => a.Id)).Result;
                SearchHelper.AddToIndex(indexClient, articlesToUpdate);
            }

            if (isUnpublishEvent)
            {
                var articlesToRemove = effectedArticles.Select(a => new Article { ID = a.Id });
                SearchHelper.RemoveFromIndex(indexClient, articlesToRemove);
            }

            return "Updated index";
        }
    }
}