using Kontent_Azure_Search_Demo.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Kontent_Azure_Search_Demo.Controllers
{
    public class ArticleController : Controller
    {
        private readonly KontentHelper kontentHelper;

        public ArticleController(IConfiguration configuration)
        {
            kontentHelper = new KontentHelper(configuration);
        }

        [Route("article/{urlPattern}")]
        public async Task<IActionResult> Detail(string urlPattern)
        {
            var item = await kontentHelper.GetArticleByUrlPattern(urlPattern);
            return View(item);
        }
    }
}