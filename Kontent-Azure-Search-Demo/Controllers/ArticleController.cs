using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontent_Azure_Search_Demo.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Kontent_Azure_Search_Demo.Controllers
{
    public class ArticleController : Controller
    {
        private readonly string projectId;

        public ArticleController(IConfiguration configuration)
        {
            projectId = configuration["KontentProjectID"];
        }

        [Route("article/{urlPattern}")]
        public async Task<IActionResult> Detail(string urlPattern)
        {
            var item = await KontentHelper.GetArticleByUrlPattern(projectId, urlPattern);
            return View(item);
        }
    }
}