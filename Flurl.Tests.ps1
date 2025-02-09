Describe 'New-FlQuery' {

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

# $splat = @{
#     uri = 'https://www.google.com/not and there/'
#     username = 'username'
#     hostname = 'aka.no'
#     port = 80
#     fragment = 'my fragment'
#     scheme = 'http'
#     password = 'password' | ConvertTo-SecureString -AsPlainText -Force
#     path = 'i/am','/here/','/and there
    
#     /','index.html'
#     # AsUriBuilder = $true
#     # EncodeSpaceAsPlus = $true
#     # Query = @(
#     #     @{
#     #     'q' = 'a and b'
#     #     'oq' = 'a,b,c'
#     # },        @{
#     #     'q' = 'a and b'
#     #     'oq' = 'a,b,c'
#     # })
#     Query = @{
#         'q' = 'a and b'
#         'oq' = 'a,b,c'
#     }
# }
# New-Flurl @splat

# New-FlQuery -Query $s
# New-FlQuery -Query $splat.Query -AsString

# $b = [Collections.specialized.NameValueCollection]::new()
# $b.Add('empty',$null)
# $b.Add('a','a')
# New-FlQuery -Query $b  -NullValueHandling Ignore -AsString -EncodeSpaceAsPlus
# New-FlQuery -Query $b  -NullValueHandling Remove -AsString -EncodeSpaceAsPlus
# New-FlQuery -Query $b  -NullValueHandling NameOnly -AsString -EncodeSpaceAsPlus

# New-FlQuery -Query @{
#     'q' = 'q'
#     'empty' = $null
# } -NullValueHandling Ignore -AsString
# New-FlQuery -Query @{
#     'q' = 'q'
#     'empty' = $null
# } -NullValueHandling NameOnly -asssssssssssssssss
# New-FlQuery -Query @{
#     'q' = 'q'
#     'empty' = $null
# } -NullValueHandling Ignore
# New-FlQuery -Query @{
#     'q' = 'q'
#     'empty' = ''
# } -NullValueHandling Ignore


# New-FlQuery -Query @(
#         @{
#         'q' = 'a and b'
#         'oq' = 'a,b,c'
#     },        @{
#         'q' = 'a and b'
#         'oq' = 'a,b,c'
#     })
