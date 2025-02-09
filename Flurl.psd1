@{
    RootModule        = 'lib/Flurl.Cmdlets.dll'
    ModuleVersion     = '0.1.0'
    GUID              = 'b797daf2-bf1c-47a1-b9dd-3b000a19bed4'
    Author            = 'Jason Vercellone'
    CompanyName       = 'vercellone'
    Copyright         = '(c) 2025 Jason Vercellone. All rights reserved.'
    Description       = 'Fluent URL Building with Flurl.'
    PowerShellVersion = '7.4'
    # Assemblies that must be loaded prior to importing this module
    # RequiredAssemblies = @()
    # Format files (.ps1xml) to be loaded when importing this module
    # FormatsToProcess = @()
    FunctionsToExport = @()
    CmdletsToExport   = @(
        'New-FlQuery',
        'New-Flurl'
    )
    AliasesToExport   = @()
    FileList          = @(
        'lib/Flurl.Cmdlets.dll',
        'lib/Flurl.dll'
    )
    PrivateData       = @{
        PSData = @{
            Tags       = @('Flurl', 'ParseQueryString', 'QueryString', 'Uri', 'Url')
            # A URL to the license for this module.
            # LicenseUri = ''
            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/vercellone/Flurl'
            # A URL to an icon representing this module.
            # IconUri = ''
        }
    }
}
