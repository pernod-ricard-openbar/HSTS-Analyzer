# HSTS Analyzer
Strict Transport Security initiative to simplify collect of HSTS headers.

## API
Endpoint : `https://api.openbar.pernod-ricard.io/v1/hsts/analyze/?url=`**{url}**`&followRedirects=`**{true|false}**
- **url** : Encoded format is expected (eg: https%3A%2F%2Fpernod-ricard.com)
- **followRedirects** : Optional parameter, defaults to true

Example : 
<https://api.openbar.pernod-ricard.io/v1/hsts/analyze/?url=https%3A%2F%2Fpernod-ricard.com&followRedirects=true>

Payload returned :
```
{
  "url": "https://pernod-ricard.com/",
  "urlAfterRedirects": "https://www.pernod-ricard.com/en",
  "grade": "C",
  "headerExists": true,
  "maxAge": 31622400,
  "includeSubDomains": false,
  "preload": false,
  "preloadStatus": "unknown"
}
```

## Scoring
The API returns a grade. Here is how it is calculated:

| Grade  | Max-Age                        | IncludeSubDomaines  | Preload  | Preload status at <br> hstspreload.org  | Comment |
|:------:|:------------------------------:|:-------------------:|:--------:|:---------------------------------------:|:--------|
| A+     | max-age >= 1 year              | yes                 | yes      | preloaded                               |         |
| A      | max-age >= 1 year              | yes                 | yes      | not preloaded                           |         |
| B      | max-age >= 1 year              | yes                 | no       | n/a                                     |         |
| C      | max-age >= 1 year              | no                  | any      | n/a                                     |         |
| D      | 0 < max-age < 1 year           | yes                 | any      | n/a                                     | if max-age < 1 year preloading is anyway useless because a prerequisite is to have max-age >= 1 year|
| E      | 0 < max-age < 1 year           | no                  | any      | n/a                                     |         |
| F      | max-age = 0 or header missing  | any                 | any      | n/a                                     |         |

## OpenBar initiative
This project falls under the bucket of the [OpenBar initiative](https://www.openbar.pernod-ricard.io/) at Pernod Ricard which aims to promote Open Source and Open Data intiatives.