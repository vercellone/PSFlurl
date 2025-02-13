using System;
using System.Management.Automation;
using System.Net;
using System.Security;
using Flurl;
using PSFlurl.Attributes;

namespace PSFlurl.Cmdlets {

    [Cmdlet(VerbsCommon.New, "Flurl")]
    [Alias("Get-Flurl")]
    [OutputType(typeof(string))]
    [OutputType(typeof(Uri))]
    [OutputType(typeof(Url))]
    public class NewFlurl : PSCmdlet {
        private Url _url = new Url();

        /// <summary>
        /// <para type="description">The base URI to start with.</para>
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Uri")]
        public Url Url { get; set; }

        /// <summary>
        /// <para type="description">The host name of the URI. Host is a reserved keyword in PowerShell, so this parameter is named HostName.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [Alias("Host")]
        [ValidateNotNullOrEmpty]
        public string HostName { get; set; }

        /// <summary>
        /// <para type="description">The path segments to append to the URI.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        /// <summary>
        /// <para type="description">The scheme of the URI (e.g., http, https).</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Scheme { get; set; }

        /// <summary>
        /// <para type="description">The port number of the URI.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateRange(1, 65535)]
        public int? Port { get; set; }

        /// <summary>
        /// <para type="description">The query parameters to add to the URI.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        [FluentQueryTransform()]
        public QueryParamCollection Query { get; set; }

        /// <summary>
        /// <para type="description">The fragment to add to the URI.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [AllowEmptyString]
        [AllowNull]
        public string Fragment { get; set; }

        /// <summary>
        /// <para type="description">The user name for the URI.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">The password for the URI.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SecureString Password { get; set; }

        /// <summary>
        /// <para type="description">Specifies how to handle null values in the query parameters.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public NullValueHandling NullValueHandling { get; set; } = NullValueHandling.Remove;

        /// <summary>
        /// <para type="description">Specifies whether to output the query parameters as a string with spaces as plus signs.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter EncodeSpaceAsPlus { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to output the Flurl.Url as a System.Uri object.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter AsUri { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to output the Flurl.Url as a string.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter AsString { get; set; }

        protected override void BeginProcessing() {
            base.BeginProcessing();
        }

        protected override void ProcessRecord() {
            if (Url != null) {
                _url = Url;
            }

            // Map simple parameters to properties
            var parameters = new (string Name, Action Action)[]
            {
                (nameof(HostName), () => _url.Host = HostName),
                (nameof(Scheme), () => _url.Scheme = Scheme),
                (nameof(Port), () => _url.Port = Port),
                (nameof(Fragment), () => _url.Fragment = Fragment),
                (nameof(Path), () => _url.AppendPathSegments(Path))
            };
            foreach ((string Name, Action Action) parameter in parameters) {
                if (MyInvocation.BoundParameters.ContainsKey(parameter.Name)) {
                    parameter.Action();
                }
            }

            // Set UserInfo
            // We don't accept PSCredential because it complicates the
            // UserName with no Password scenario.
            if (MyInvocation.BoundParameters.ContainsKey(nameof(UserName))) {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(Password))) {
                    string password = new NetworkCredential(string.Empty, Password).Password;
                    _url.UserInfo = $"{UserName}:{password}";
                }
                else {
                    _url.UserInfo = UserName;
                }
            }

            if (Query != null) {
                foreach ((string Name, object Value) in Query) {
                    object val = Value != null && string.IsNullOrWhiteSpace($"{Value}") ? null : Value;
                    _url.QueryParams.Add(Name, val, false, NullValueHandling);
                }
            }
        }

        protected override void EndProcessing() {
            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent) {
                WriteObject(_url.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else if (AsUri.IsPresent) {
                WriteObject(_url.ToUri());
            }
            else {
                WriteObject(_url);
            }
        }
    }
}