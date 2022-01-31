using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hsts
{
    public static class Hello
    {
        [FunctionName("Hello")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "hello")] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("This is HSTS by Pernod Ricard");
        }
    }
}
