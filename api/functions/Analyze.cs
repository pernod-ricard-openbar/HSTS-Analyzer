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
        private readonly HstsService _hstsService;
        private readonly bool _followRedirectsDefaultValue = false;

        public Analyze(HstsService hstsService)
        {
            _hstsService = hstsService;
        }

        [FunctionName("Analyze")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "analyze")] HttpRequest req,
            ILogger log)
        {
            // Get query string parameter url
            string url = String.Empty;
            if (req.Query.TryGetValue("url", out var urlStringValues))
            {
                // Decode URL
                url = HttpUtility.UrlDecode(urlStringValues[0]);
            }
            else
            {
                // If url parameter is missing we return an error message
                return new BadRequestObjectResult("Query string parameter 'url' is missing.");
            }

            // Get query string parameter followRedirects
            bool followRedirects = _followRedirectsDefaultValue;
            if (req.Query.TryGetValue("followRedirects", out var followRedirectsStringValues))
            {
                bool.TryParse(followRedirectsStringValues[0], out followRedirects);
            }

            // Analyze Hsts
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                var hstsResult = await _hstsService.AnalyzeAsync(uri, followRedirects);
                return new OkObjectResult(hstsResult);
            }
            else
            {
                return new BadRequestObjectResult("Failed to parse query string parameter 'url'. Expecting url={http|https}://{domain}/{?path}.");
            }
        }
    }
}
