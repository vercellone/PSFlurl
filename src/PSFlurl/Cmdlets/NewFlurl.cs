using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Security;
using Flurl;
using PSFlurl.Attributes;
using PSFlurl.Utilities;
using PSFlurl.Extensions;

namespace PSFlurl.Cmdlets {

    [Cmdlet(VerbsCommon.New, "Flurl")]
    [Alias("Get-Flurl")]
    [OutputType(typeof(string))]
    [OutputType(typeof(Uri))]
    [OutputType(typeof(Url))]
    public class NewFlurl : PSCmdlet {
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
        public object Query { get; set; }

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

        protected override void ProcessRecord() {
            if (Url == null) {
                Url = new Url();
            }

            // Map simple parameters to properties
            var parameters = new (string Name, Action Action)[]
            {
                (nameof(HostName), () => Url.Host = HostName),
                (nameof(Scheme), () => Url.Scheme = Scheme),
                (nameof(Port), () => Url.Port = Port),
                (nameof(Fragment), () => Url.Fragment = Fragment),
                (nameof(Path), () => Url.AppendPathSegments(Path))
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
                    Url.UserInfo = $"{UserName}:{password}";
                }
                else {
                    Url.UserInfo = UserName;
                }
            }

            // Query accepts a variety of types, which are mostly handled
            // by the the FluentQueryTransformAttribute
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Query))) {
                if (Query is QueryParamCollection collection) {
                    IEnumerable<KeyValuePair<string, object>> kvpEnumerable = QueryParamCollectionConverter.ConvertToKeyValuePairs(collection);
                    Url.QueryParams.AddRange(kvpEnumerable, this.NullValueHandling);
                }
                else if (Query is IEnumerable<KeyValuePair<string, object>> kvpEnumerable) {
                    Url.QueryParams.AddRange(kvpEnumerable, this.NullValueHandling);
                }
                else {
                    WriteError(new ErrorRecord(new ArgumentException($"Query ({Query.GetType().FullName}) must be string(s), IDictionary(s), NameValueCollection, QueryParamCollection, or IEnumerable<KeyValuePair<string, object>>"), "InvalidArgument", ErrorCategory.InvalidArgument, Query));
                }
            }
            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent) {
                WriteObject(Url.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else if (AsUri.IsPresent) {
                WriteObject(Url.ToUri());
            }
            else {
                WriteObject(Url);
            }
        }
    }
}