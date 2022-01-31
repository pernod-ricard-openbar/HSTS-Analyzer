using System;

namespace Hsts {

    #nullable enable
    public class HstsResult{

        public HstsResult(Uri uri) {
            Url = uri.ToString();
        }

        public string Url { get; set; }
        public string? Grade { get; set; }
        public bool HeaderExists { get; set; }
        public int? MaxAge { get; set; }
        public bool? IncludeSubDomains { get; set; }
        public bool? Preload { get; set; }
        public string? PreloadStatus { get; set; }

    }
}