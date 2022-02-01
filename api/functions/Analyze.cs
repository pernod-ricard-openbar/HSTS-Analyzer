using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Web;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Hsts
{
    public class Analyze
    {
        private readonly HstsService _hstsService;
        private readonly bool _followRedirectsDefaultValue = false;

        public Analyze(HstsService hstsService) {
            _hstsService = hstsService;
        }

        [FunctionName("Analyze")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "analyze/{url}")] HttpRequest req,
            string url,
            ILogger log)
        {
            // Get query string parameter followRedirects
            bool followRedirects = _followRedirectsDefaultValue;
            StringValues headersFollowRedirects;
            if (req.Query.TryGetValue("followRedirects", out headersFollowRedirects)) {
                var headerFollowRedirects = headersFollowRedirects.FirstOrDefault();
                bool.TryParse(headerFollowRedirects, out followRedirects);
            }

            // Decode URL
            url = HttpUtility.UrlDecode(url);

            // Analyze Hsts
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri)) {
                var hstsResult = await _hstsService.AnalyzeAsync(uri, followRedirects);
                return new OkObjectResult(hstsResult);
            }
            else {
                return new StatusCodeResult(400);
            }
        }
    }
}
