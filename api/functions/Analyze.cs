using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Hsts
{
    public class Analyze
    {
        private HstsService _hstsService;

        public Analyze(HstsService hstsService) {
            _hstsService = hstsService;
        }

        [FunctionName("Analyze")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "analyze/{url}")] HttpRequest req,
            string url,
            ILogger log)
        {
            // Decode URL
            url = HttpUtility.UrlDecode(url);

            // Analyze Hsts
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri)) {
                var hstsResult = await _hstsService.AnalyzeAsync(uri);
                return new OkObjectResult(hstsResult);
            }
            else {
                return new StatusCodeResult(400);
            }
        }
    }
}
