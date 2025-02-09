# Flurl

This Flurl PowerShell module was born out of frustration with the limitations of .NET's `Uri`, `UriBuilder`, and `HttpUtility.ParseQueryString`. These native tools fall short when dealing with modern URL manipulation needs, especially with regard to encoding paths and query strings. After evaluating various options, the [Flurl](https://flurl.dev) library emerged as the clear solution, offering an elegant and powerful API for URL manipulation.

## Why Flurl?

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

## Examples

### Working with Query Parameters

```powershell
# Simple Hashtable
New-Flurl -Uri 'https://api.example.com' -Query @{
    search = 'powershell module'
    filter = 'active'
} -AsString

https://api.example.com/?filter=active&search=powershell%20module
```

#### Using `+` encoding for spaces

```powershell
New-Flurl -Uri 'https://api.example.com' -Query @{
    search = 'powershell module'
} -EncodeSpaceAsPlus -AsString

https://api.example.com/?search=powershell+module
```

### Handling Null Values

#### Remove (Default)
```powershell
New-FlQuery -Query @{
    required = 'value'
    optional = $null
} -NullValueHandling Remove -AsString

required=value
```

#### Ignore
```powershell
New-FlQuery -Query @{
    required = 'value'
    optional = $null
} -NullValueHandling Ignore -AsString

optional=&required=value
```

#### NameOnly
```powershell
New-FlQuery -Query @{
    required = 'value'
    optional = $null
} -NullValueHandling NameOnly -AsString

optional&required=value
```

### Duplicate Query Parameters

#### Using array of hashtables
```powershell
New-FlQuery -Query @(
    @{ state = 'OPEN' },
    @{ staet = 'MERGED' }
) -AsString

state=OPEN&state=MERGED
```

#### Using NameValueCollection
```powershell
$nvc = [System.Collections.Specialized.NameValueCollection]::new()
$nvc.Add('tag', 'powershell')
$nvc.Add('tag', 'module')
New-FlQuery -Query $nvc -AsString

tag=powershell&tag=module
```

### Complex URL Construction

```powershell
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

https://api.example.com/v2/search?q=test+query&sort=relevance&filter=active#results
```

## Installation

### Install from PowerShellGallery

```powershell
Install-Module -Name Flurl -Repository PSGallery -Scope CurrentUser
```

### Import the module

```powershell
Import-Module -Name Flurl
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Credits

Special thanks to the creators and maintainers of [Flurl](https://flurl.dev). Their excellent library makes this PowerShell module possible by providing a robust foundation for URL manipulation that goes beyond the capabilities of the .NET framework's built-in classes.

## License

This project is licensed under the MIT License - see the LICENSE file for details.