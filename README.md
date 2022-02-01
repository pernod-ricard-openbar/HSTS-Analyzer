# HSTS Analyzer
Strict Transport Security initiative to simplify collect of HSTS headers.

## API
Endpoint : `https://api.openbar.pernod-ricard.io/v1/hsts/analyze/`**{url}**`?followRedirects=`**{true|false}**
- **url** : Encoded format is expected (eg: https%3A%2F%2Fpernod-ricard.com)
- **followRedirects** : Optional parameter, defaults to false

Example : 
```
<https://api.openbar.pernod-ricard.io/v1/hsts/analyze/https%3A%2F%2Fpernod-ricard.com?followRedirects=true>
```

Payload returned :
```
{
  "url": "http://pernod-ricard.com/",
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

