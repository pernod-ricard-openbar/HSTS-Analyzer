using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Hsts {

    #nullable enable
    public class HstsService {

        private HttpClient _httpClientFollowRedirects;
        private HttpClient _httpClientDoNotFollowRedirects;

        public HstsService(IHttpClientFactory httpClientFactory) {
            _httpClientFollowRedirects = httpClientFactory.CreateClient("HttpClientFollowRedirects");
            _httpClientDoNotFollowRedirects = httpClientFactory.CreateClient("HttpClientDoNotFollowRedirects");
        }

        public HttpClient GetHttpClient(bool followRedirects = false) {
            return followRedirects ? _httpClientFollowRedirects : _httpClientDoNotFollowRedirects;
        }

        public async Task<HstsResult> AnalyzeAsync(Uri uri, bool followRedirects = false) {
            
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

                if (response == null ) return hstsResult;

                // If we follow the redirections, we might need to update the url accordingly
                if (followRedirects == true) {
                    uri = response?.RequestMessage?.RequestUri ?? uri;
                    hstsResult.Url = uri.ToString();
                }
            }
            catch(Exception) {
                return hstsResult;
            }
            
            if (response != null && !response.Headers.Contains(headerName)) {       
                hstsResult.Grade = HstsGrade.F;    
                return hstsResult;
            }
            else {
                hstsResult.HeaderExists = true;
                hstsResult.IncludeSubDomains = false; // will eventually be set to true later
                hstsResult.Preload = false; // will eventually be set to true later
                IEnumerable<string>? headersSts;
                if (response != null && response.Headers.TryGetValues(headerName, out headersSts)) {
                    var headerValue = headersSts.FirstOrDefault(String.Empty);
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
            }
            // PreloadStatus
            hstsResult.PreloadStatus = await GetPreloadStatusAsync(uri);

            // Grade 
            hstsResult.Grade = ComputeGrade(hstsResult);

            // Return the result
            return hstsResult;
        }

        // Compute grade
        private string? ComputeGrade(HstsResult r) {
            int oneYear = 31536000;
            if (r.MaxAge != null && r.MaxAge >= oneYear) {
                if ((r.IncludeSubDomains ?? false) == true) { // IncludeSubDomains == true
                    if ((r.Preload ?? false) == true) { // Preload == true
                        if ((r.PreloadStatus ?? String.Empty) == "preloaded") { //PreloadStatus == preloaded
                            return HstsGrade.APlus;
                        }
                        else { // PreloadStatus != preloaded
                            return HstsGrade.A;
                        }
                    }
                    else { // Preload == false
                        return HstsGrade.B;
                    }
                }
                else { // IncludeSubDomains == false
                    return HstsGrade.C;
                }
            }
            else if (r.MaxAge != null && r.MaxAge > 0 && r.MaxAge < oneYear) {
                if ((r.IncludeSubDomains ?? false) == true) { // IncludeSubDomains == true
                   return HstsGrade.D;
                }
                else { // IncludeSubDomains == false
                    return HstsGrade.E;
                }
            }
            else if (r.MaxAge == null || r.MaxAge == 0) {
                    return HstsGrade.F;
            }
            else {
                // Normally we should not endup in this case
                return null;
            }
        }

        // Gets the preload status from hstspreload.org
        private async Task<string> GetPreloadStatusAsync(Uri uri) {
            string defaultStatus = "unknown";
            string resultStatus = defaultStatus;
            string domain = uri.Host;
            string url = $"https://hstspreload.org/api/v2/status?domain={domain}";

            try {
                var response = await GetHttpClient(followRedirects: true).GetAsync(url);
                string jsonString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(jsonString);
                resultStatus = jsonNode?["status"]?.ToString() ?? defaultStatus;
            }
            catch (Exception) {
                return defaultStatus;
            }
            return resultStatus;
        }
    }
}