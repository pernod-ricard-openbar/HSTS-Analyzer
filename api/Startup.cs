using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Hsts.Startup))]

namespace Hsts
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("HttpClientFollowRedirects")
                .ConfigurePrimaryHttpMessageHandler(
                    x => new HttpClientHandler() {
                        AllowAutoRedirect = true
                    }
                );
            builder.Services.AddHttpClient("HttpClientDoNotFollowRedirects")
                .ConfigurePrimaryHttpMessageHandler(
                    x => new HttpClientHandler() {
                        AllowAutoRedirect = false
                    }
                );
            builder.Services.AddScoped<HstsService>();
        }
    }
}