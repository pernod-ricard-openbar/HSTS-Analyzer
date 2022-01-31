using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Hsts {

    public class HstsService {

        private HttpClient _httpClientFollowRedirects;
        private HttpClient _httpClientDoNotFollowRedirects;

        public HstsService(IHttpClientFactory httpClientFactory) {
            _httpClientFollowRedirects = httpClientFactory.CreateClient("HttpClientFollowRedirects");
            _httpClientDoNotFollowRedirects = httpClientFactory.CreateClient("HttpClientDoNotFollowRedirects");
        }

        public HttpClient GetHttpClient(bool followRedirects = true) {
            return followRedirects ? _httpClientFollowRedirects : _httpClientDoNotFollowRedirects;
        }

        public async Task<HstsResult> AnalyzeAsync(Uri uri, bool followRedirects = true) {
            
            HstsResult hstsResult = new HstsResult(uri) {
                Grade = null,
                HeaderExists = false,
                MaxAge = null,
                IncludeSubDomains = null,
                Preload = null,
                PreloadStatus = null
            };
            string headerName = "strict-transport-security";

            HttpResponseMessage response;
            try {
                response = await GetHttpClient(followRedirects).GetAsync(uri);
            }
            catch(Exception) {
                return hstsResult;
            }
            
            if (!response.Headers.Contains(headerName)) {              
                return hstsResult;
            }
            else {
                hstsResult.HeaderExists = true;
                hstsResult.IncludeSubDomains = false; // will eventually be set to true later
                hstsResult.Preload = false; // will eventually be set to true later
                var headerValue = response.Headers.GetValues(headerName).FirstOrDefault();
                foreach (var element in headerValue.Trim().Split(';')) {
                    string trimmedElement = element.Trim();
                    // Max-Age
                    if (trimmedElement.StartsWith("max-age=")){
                        int maxAge;
                        if (int.TryParse(trimmedElement.Replace("max-age=", ""), out maxAge)) {
                            hstsResult.MaxAge = maxAge;
                        }
                        else {
                            hstsResult.MaxAge = null;
                        }
                    }
                    // includeSubDomains
                    if (trimmedElement.Equals("includeSubDomains")) hstsResult.IncludeSubDomains = true;
                    // preload
                    if (trimmedElement.Equals("preload")) hstsResult.Preload = true;
                }
            }
            // PreloadStatus
            hstsResult.PreloadStatus = await GetPreloadStatusAsync(uri);

            // Grade 
            hstsResult.Grade = ComputeGrade(hstsResult);

            // Return the result
            return hstsResult;
        }

        // Compute grate
        private string ComputeGrade(HstsResult hstsResult) {
            return "X";
        }

        // Gets the preload status from hstspreload.org
        private async Task<string> GetPreloadStatusAsync(Uri uri) {
            string defaultStatus = "unknown";
            string resultStatus = defaultStatus;
            string domain = uri.Host;
            string url = $"https://hstspreload.org/api/v2/status?domain={domain}";

            try {
                var response = await GetHttpClient().GetAsync(url);
                string jsonString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(jsonString);
                resultStatus = jsonNode["status"]?.ToString() ?? defaultStatus;
            }
            catch (Exception) {
                return defaultStatus;
            }
            return resultStatus;
        }
    }
}