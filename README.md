# PSFlurl

[![CI](https://github.com/vercellone/PSFlurl/actions/workflows/ci.yml/badge.svg)](https://github.com/vercellone/PSFlurl/actions/workflows/ci.yml)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSFlurl.svg?style=flat-square&color=012456&label=PowerShell%20Gallery)](https://www.powershellgallery.com/packages/PSFlurl)
[![Try it Now](https://img.shields.io/badge/Try_It_Now-Codespaces-161B22?style=flat-square&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=929630232)

PSFlurl was born out of frustration with the limitations of .NET's `Uri`, `UriBuilder`, `HttpUtility.ParseQueryString`, and `HttpQSCollection.ToString()`. These native tools fall short when dealing with modern URL manipulation needs, especially with regard to encoding paths and query strings. After evaluating various options, the [Flurl](https://flurl.dev) library emerged as the clear solution, offering an elegant and powerful API for URL manipulation.

## Why PSFlurl?

As detailed in [Flurl documentation](https://flurl.dev/docs/fluent-url/), Flurl addresses several key shortcomings of the .NET framework's URL handling.

- Percent-encoded-triplets are ALWAYS UPPER CASE
- Percent-encoded-triplets are NEVER re-encoded
- Fragments are encoded
- Query keys and values are encoded
- Opt-in to encoding spaces as `+` (default is `%20`)
- 3 NullValueHandling options
  - **Ignore** Don't add to query string, but leave any existing value unchanged.
  - **NameOnly** Set as name without value in query string
  - **Remove** (Default) Don't add to query string, remove any existing value.
- Simpler and more powerful support for duplicate query parameter keys

The PowerShell module exposes all of the above via the following cmdlets

## Cmdlets

- `New-Flurl`: Creates and manipulates URLs with a fluent interface
- `New-FlQuery`: Constructs query strings with advanced options

> [!TIP]
> `Get-Flurl` and `Get-FlQuery` aliases are exported.
> `Flurl` and `FlQuery` work, too!

## Usage

<details open>
<summary>Url</summary>

```powershell copy
$splat = @{
    Uri               = 'https://api.example.com'
    Path              = @('v2', 'search')
    Query             = [ordered]@{
        q      = 'test query'
        filter = 'active'
        sort   = 'relevance'
    }
    Fragment          = 'results'
    EncodeSpaceAsPlus = $true
    AsString          = $true
}
New-Flurl @splat

# https://api.example.com/v2/search?q=test+query&sort=relevance&filter=active#results
```
</details>

<details>
<summary>Repeated Names</summary>

### Array of Hashtables
```powershell copy
New-FlQuery -Query @(
    @{ state = 'OPEN' },
    @{ state = 'MERGED' }
) -AsString

# state=OPEN&state=MERGED
```

### NameValueCollection
```powershell copy
$nvc = [System.Collections.Specialized.NameValueCollection]::new()
$nvc.Add('tag', 'powershell')
$nvc.Add('tag', 'module')
New-FlQuery -Query $nvc -AsString

# tag=powershell&tag=module
```
</details>

<details>
<summary>NullOrEmpty Values</summary>

### Remove (Default)
```powershell copy
New-FlQuery -Query @{
    required = 'value'
    optional = $null
} -NullValueHandling Remove -AsString

# required=value
```

### Ignore
```powershell copy
New-FlQuery -Query @{
    required = 'value'
    optional = $null
} -NullValueHandling Ignore -AsString

# optional=&required=value
```

### NameOnly
```powershell copy
New-FlQuery -Query @{
    required = 'value'
    optional = $null
} -NullValueHandling NameOnly -AsString

# optional&required=value
```
</details>

<details>
  <summary>`+` encoding spaces</summary>

  ```powershell copy
  New-Flurl -Uri 'https://api.example.com' -Query @{
      search = 'powershell module'
  } -EncodeSpaceAsPlus -AsString

  # https://api.example.com/?search=powershell+module
  ```

</details>

<details>
  <summary>Fluent</summary>

  ```powershell copy
  (Flurl 'https://some-api.com:88').
  AppendPathSegment('endpoint').
  SetFragment('after-hash').
  AppendQueryParam(@{
        api_key = 'MyApiKey'
         max_results = 20
         q = 'I''ll get encoded!'
  }).
  ToString()

  # https://some-api.com/endpoint?q=I%27ll%20get%20encoded%21&api_key=MyApiKey&max_results=20#after-hash
  ```

</details>

<details>
  <summary>Utility Methods</summary>

  ```powershell copy

  $url = [Flurl.Url]::Combine('http://foo.com/', '/too/', '/many/', '/slashes/', 'too', 'few?', 'x=1', 'y=2')
  $url.ToString()

  # http://foo.com/too/many/slashes/too/few?x=1&y=2

  [Flurl.Url]::IsValid($url)

  # True
  ```

</details>

## Installation

### Install from PowerShellGallery

```powershell copy
Install-Module -Name PSFlurl -Repository PSGallery -Scope CurrentUser
```

### Import the module

```powershell copy
Import-Module -Name PSFlurl
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Credits

Special thanks to the creators and maintainers of [Flurl](https://flurl.dev). Their excellent library makes this PowerShell module possible by providing a robust foundation for URL manipulation that goes beyond the capabilities of the .NET framework's built-in classes.

## License

This project is licensed under the MIT License - see the LICENSE file for details.