using Kontent_Azure_Search_Demo.Helpers;
using Kontent_Azure_Search_Demo.KontentWebhookModels;
using Kontent_Azure_Search_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kontent_Azure_Search_Demo.Controllers
{
    public class SearchController : Controller
    {
        private readonly KontentHelper kontentHelper;
        private readonly KontentWebhookHelper kontentWebhookHelper;
        private readonly SearchHelper searchHelper;

        public SearchController(IConfiguration configuration)
        {
            kontentHelper = new KontentHelper(configuration);
            searchHelper = new SearchHelper(configuration);
            kontentWebhookHelper = new KontentWebhookHelper(configuration);
        }

        public IActionResult Create()
        {
            searchHelper.DeleteIndexIfExists();
            searchHelper.CreateIndex<Article>();
            
            ViewData["Message"] = "Index deleted/created.";

            return View("Index");
        }

        public IActionResult Index(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) {
                return View();
            }

            var results = searchHelper.QueryIndex<Article>(searchText);

            return View(results);
        }

        public IActionResult Initialize()
        {
            var articles = kontentHelper.GetArticlesForSearch().Result;
            searchHelper.AddToIndex<Article>(articles);

            ViewData["Message"] = "Index initialized.";
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

            var isValid = kontentWebhookHelper.ValidateWebhook(Request.Headers["X-KC-Signature"], bodyRaw);
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
                var articlesToUpdate = kontentHelper.GetArticlesForSearch(effectedArticles.Select(a => a.Id)).Result;
                searchHelper.AddToIndex(articlesToUpdate);
            }

            if (isUnpublishEvent)
            {
                var articlesToRemove = effectedArticles.Select(a => new Article { ID = a.Id });
                searchHelper.RemoveFromIndex(articlesToRemove);
            }

            return "Updated index";
        }
    }
}