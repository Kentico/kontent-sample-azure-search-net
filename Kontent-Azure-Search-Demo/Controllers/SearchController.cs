using Kontent_Azure_Search_Demo.Helpers;
using Kontent_Azure_Search_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
            if (string.IsNullOrWhiteSpace(searchText))
            {
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
            
            var processedWebhook = kontentWebhookHelper.ValidateAndProcessWebhook(Request.Headers["X-KC-Signature"], bodyRaw);
            if (!processedWebhook.IsValid)
            {
                return processedWebhook.ErrorMessage;
            }

            if (processedWebhook.Operation == "publish")
            {
                var articlesToUpdate = kontentHelper.GetArticlesForSearch(processedWebhook.Articles.Select(a => a.Id)).Result;
                searchHelper.AddToIndex(articlesToUpdate);
            }

            if (processedWebhook.Operation == "unpublish")
            {
                var articlesToRemove = processedWebhook.Articles.Select(a => new Article { ID = a.Id });
                searchHelper.RemoveFromIndex(articlesToRemove);
            }

            return "Updated index";
        }
    }
}