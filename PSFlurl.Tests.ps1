Describe 'PSFlurl' {

    Context 'New-FlQuery' {

        It 'should handle Query of type [IDictionary]' {
            $query = @{
                '$top'  = 100
                'empty' = $null
            }
            $result = New-FlQuery -Query $query
            $result.GetAll('$top') | Should -Be 100
            $result.GetAll('empty') | Should -BeNullOrEmpty
        }

        It 'should handle Query of type [IDictionary[]]' {
            $query = @(
                @{
                    '$top'  = 100
                    'state' = 'OPEN'
                },
                @{
                    'state' = 'MERGED'
                }
            )
            $result = New-FlQuery -Query $query
            $result.GetAll('$top') | Should -Be 100
            $result.GetAll('state') | Should -Be ('OPEN', 'MERGED')
        }
    
        It 'should handle Query of type [IEnumerable[KeyValuePair[string, object]]]' {
            $query = [System.Collections.Generic.List[System.Collections.Generic.KeyValuePair[string, object]]]::new()
            $query.Add([System.Collections.Generic.KeyValuePair[string, object]]::new('$top', 100))
            $query.Add([System.Collections.Generic.KeyValuePair[string, object]]::new('empty', $null))
            $query.Add([System.Collections.Generic.KeyValuePair[string, object]]::new('state', 'OPEN'))
            $query.Add([System.Collections.Generic.KeyValuePair[string, object]]::new('state', 'MERGED'))
            $result = New-FlQuery -Query $query
            $result.GetAll('$top') | Should -Be 100
            $result.GetAll('empty') | Should -BeNullOrEmpty
            $result.GetAll('state') | Should -Be ('OPEN', 'MERGED')
        }

        It 'should handle Query of type [NameValueCollection]' {
            # This produces a System.Collections.Specialized.NameValueCollection
            $query = [System.Web.HttpUtility]::ParseQueryString('$top=100&empty=&state=OPEN&state=MERGED')
            $result = New-FlQuery -Query $query
            $result.GetAll('$top') | Should -Be 100
            $result.GetAll('empty') | Should -BeNullOrEmpty
            $result.GetAll('state') | Should -Be ('OPEN', 'MERGED')
        }

        It 'should handle Query of type [string]' {
            $result = New-FlQuery -Query 'filter=my name&$top=100&empty=&state=OPEN&state=MERGED'
            $result.GetAll('filter') | Should -Be 'my name'
            $result.GetAll('$top') | Should -Be 100
            $result.GetAll('empty') | Should -BeNullOrEmpty
            $result.GetAll('state') | Should -Be ('OPEN', 'MERGED')
        }

        It 'should handle Query of type [string[]]' {
            $result = New-FlQuery -Query @('filter=my name&$top=100&empty=&state=OPEN&state=MERGED', '&filter=your name&$top=200') -NullValueHandling Remove
            # [string]::Join('&',@('filter=my name&$top=100&empty=&state=OPEN&state=MERGED', 'filter=your name&$top=200'))

            $result.GetAll('filter') | Should -Be ('my name', 'your name')
            $result.GetAll('$top') | Should -Be (100, 200)
            $result.GetAll('empty') | Should -BeNullOrEmpty
            $result.GetAll('state') | Should -Be ('OPEN', 'MERGED')
        }
    }

    Context 'New-Flurl Parameter Sets' {
        BeforeAll {
            $baseUri = 'https://api.example.com'
            $queryParams = @{ key = 'value'; empty = $null }
        }

        It 'should work with NO input (default)' {
            $result = New-Flurl
            $result | Should -BeOfType ([Flurl.Url])
            $result.ToString() | Should -Be ''
        }

        It 'should work when pipeing directly from New-FlQuery | New-Flurl' {
            $result = New-FlQuery -Query $queryParams | New-Flurl -Uri $baseUri
            $result | Should -BeOfType ([Flurl.Url])
            $result.ToString() | Should -Be "$baseUri`?key=value"
        }

        It 'should throw when pipeing a stored variable $q | New-Flurl' {
            try {
                $query = New-FlQuery -Query $queryParams
                $ErrorActionPreference = 'Stop'
                $query | New-Flurl -ErrorAction Stop
                Should -Fail "Expected error was not thrown"
            }
            catch {
                $_.Exception.Message | Should -Match 'use \(prepend\) the comma operator'
            }
        }

        It 'should work when using comma operator with stored variable' {
            $query = New-FlQuery -Query $queryParams
            $result = , $query | New-Flurl -Uri $baseUri
            $result | Should -BeOfType ([Flurl.Url])
            $result.ToString() | Should -Be "$baseUri`?key=value"
        }
    }

    Context 'Type Conversion' {

        BeforeAll {
            $urlString = 'https://example.com/'
            $url = New-Flurl -Uri $urlString
        }

        
        It 'should verify converter registration' {
            # Get the converter
            $converter = [System.ComponentModel.TypeDescriptor]::GetConverter([Flurl.Url])
                
            # Test direct converter functionality
            $uri = $converter.ConvertTo($url, [System.Uri])
            $uri.GetType() | Should -Be ([System.Uri])
            $uri.AbsoluteUri | Should -Be $urlString
        }
        
        It 'should convert directly using ConvertTo' {
            # Get the converter
            $converter = [System.ComponentModel.TypeDescriptor]::GetConverter([Flurl.Url])
                
            # Test Uri conversion
            $uri = $converter.ConvertTo($url, [System.Uri])
            $uri.GetType() | Should -Be ([System.Uri])
                
            # Test string conversion
            $str = $converter.ConvertTo($url, [string])
            $str | Should -Be $urlString
        }

        It 'should convert Flurl.Url to System.Uri' {
            $uri = [System.Uri]$url
            $uri.GetType() | Should -Be ([System.Uri])
            $uri.AbsoluteUri | Should -Be $urlString
        }
        
        It 'should convert System.Uri to Flurl.Url' {
            $uri = [System.Uri]$urlString
            $flurlUrl = [Flurl.Url]$uri
            $flurlUrl.GetType() | Should -Be ([Flurl.Url])
            $flurlUrl.ToString() | Should -Be $urlString
        }
        
        It 'should convert string to Flurl.Url' {
            $flurlUrl = [Flurl.Url]$urlString
            $flurlUrl.GetType() | Should -Be ([Flurl.Url])
            $flurlUrl.ToString() | Should -Be $urlString
        }
        
        It 'should convert Flurl.Url to string' {
            $str = [string]$url
            $str | Should -Be $urlString
        }
    }
}