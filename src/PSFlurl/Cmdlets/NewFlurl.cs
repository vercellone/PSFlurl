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

    // DefaultParameterSetName = "None" is required to allow usage with no parameters.
    // The Uri and Query ParameterSets facilitate either (but not both) to accept ValueFromPipeline.
    [Cmdlet(VerbsCommon.New, "Flurl", DefaultParameterSetName = "None")]
    [OutputType(typeof(string))]
    [OutputType(typeof(Uri))]
    [OutputType(typeof(Url))]
    public class NewFlurl : PSCmdlet {
        /// <summary>
        /// <para type="description">The base URI to start with.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Uri", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = "FlQuery", Position = 0, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = "None", Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Uri Uri { get; set; }

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
        [Parameter(ValueFromPipelineByPropertyName = true)]
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

        /// <summary>
        /// <para type="description">The query parameters to add to the URI.</para>
        /// </summary>
        [Parameter(DontShow = true, Mandatory = true, ParameterSetName = "FlQuery", ValueFromPipeline = true)]
        public QueryParamCollection FlQuery { get; set; }

        protected override void ProcessRecord() {
            Url url = Uri == null ? new Url() : new Url(Uri.ToString());

            // Handle QueryParamCollection ValueFromPipeline
            if (MyInvocation.BoundParameters.ContainsKey(nameof(FlQuery))) {
                IEnumerable<KeyValuePair<string, object>> kvpEnumerable = QueryParamCollectionConverter.ConvertToKeyValuePairs(FlQuery);
                url.QueryParams.AddRange(kvpEnumerable, this.NullValueHandling);
            }

            // Map simple parameters to properties
            var parameters = new (string Name, Action Action)[]
            {
            (nameof(HostName), () => url.Host = HostName),
            (nameof(Scheme), () => url.Scheme = Scheme),
            (nameof(Port), () => url.Port = Port),
            (nameof(Fragment), () => url.Fragment = Fragment),
            (nameof(Path), () => url.AppendPathSegments(Path))
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
                    url.UserInfo = $"{UserName}:{password}";
                }
                else {
                    url.UserInfo = UserName;
                }
            }

            // Query accepts a variety of types, which are mostly handled
            // by the the FluentQueryTransformAttribute
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Query))) {
                if (Query is QueryParamCollection collection) {
                    IEnumerable<KeyValuePair<string, object>> kvpEnumerable = QueryParamCollectionConverter.ConvertToKeyValuePairs(collection);
                    url.QueryParams.AddRange(kvpEnumerable, this.NullValueHandling);
                }
                else if (Query is IEnumerable<KeyValuePair<string, object>> kvpEnumerable) {
                    url.QueryParams.AddRange(kvpEnumerable, this.NullValueHandling);
                }
                else {
                    WriteError(new ErrorRecord(new ArgumentException($"Query ({Query.GetType().FullName}) must be string(s), IDictionary(s), NameValueCollection, QueryParamCollection, or IEnumerable<KeyValuePair<string, object>>"), "InvalidArgument", ErrorCategory.InvalidArgument, Query));
                }
            }
            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent) {
                WriteObject(url.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else if (AsUri.IsPresent) {
                WriteObject(url.ToUri());
            }
            else {
                WriteObject(url);
            }
        }
    }
}