using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Hsts
{
    public class Test
    {
        private HstsService _hstsService;

        public Test(HstsService hstsService) {
            _hstsService = hstsService;
        }

        [FunctionName("Test")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "test")] HttpRequest req,
            ILogger log)
        {
            List<HstsResult> hstsResults = new List<HstsResult>();
            hstsResults.Add(await _hstsService.AnalyzeAsync(new Uri("https://pernod-ricard.com")));
            hstsResults.Add(await _hstsService.AnalyzeAsync(new Uri("https://www.pernod-ricard.com")));
            hstsResults.Add(await _hstsService.AnalyzeAsync(new Uri("https://www.pernod-ricard.com/en")));
            hstsResults.Add(await _hstsService.AnalyzeAsync(new Uri("https://pernod-ricard.io")));

            return new OkObjectResult(hstsResults);
        }
    }
}
