using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Security;
using Flurl;
using Flurl.Cmdlets.Attributes;
using Flurl.Cmdlets.Utilities;
using Flurl.Cmdlets.Extensions;

namespace Flurl.Cmdlets
{

    [Cmdlet(VerbsCommon.New, "Flurl")]
    [OutputType(typeof(Url))]
    [OutputType(typeof(Uri))]
    public class NewFlurlCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The base URI to start with.</para>
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
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

        protected override void ProcessRecord()
        {
            var url = Uri == null ? new Url() : new Url(Uri.ToString());

            // Map simple parameters to properties
            var parameters = new (string Name, Action Action)[]
            {
            (nameof(HostName), () => url.Host = HostName),
            (nameof(Scheme), () => url.Scheme = Scheme),
            (nameof(Port), () => url.Port = Port),
            (nameof(Fragment), () => url.Fragment = Fragment),
            (nameof(Path), () => url.AppendPathSegments(Path))
            };
            foreach (var parameter in parameters)
            {
                if (MyInvocation.BoundParameters.ContainsKey(parameter.Name))
                {
                    parameter.Action();
                }
            }

            // Set UserInfo
            // We don't accept PSCredential because it complicates the
            // UserName with no Password scenario.
            if (MyInvocation.BoundParameters.ContainsKey(nameof(UserName)))
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(Password)))
                {
                    var password = new NetworkCredential(string.Empty, Password).Password;
                    url.UserInfo = $"{UserName}:{password}";
                }
                else
                {
                    url.UserInfo = UserName;
                }
            }

            // Query accepts a variety of types, which are mostly handled
            // by the the FluentQueryTransformAttribute
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Query)))
            {
                QueryParamCollection fluentQuery = new QueryParamCollection();
                if (Query is QueryParamCollection collection)
                {
                    var kvpEnumerable = QueryParamCollectionConverter.ConvertToKeyValuePairs(collection);
                    fluentQuery.AddRange(kvpEnumerable, this.NullValueHandling);
                }
                else if (Query is IEnumerable<KeyValuePair<string, object>> kvpEnumerable)
                {
                    fluentQuery.AddRange(kvpEnumerable, this.NullValueHandling);
                }
                else
                {
                    WriteError(new ErrorRecord(new ArgumentException("Query must be a string, IDictionary, NameValueCollection, array of IDictionary, or IEnumerable<KeyValuePair<string, object>>"), "InvalidArgument", ErrorCategory.InvalidArgument, Query));
                }
            }
            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent)
            {
                WriteObject(url.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else if (AsUri.IsPresent)
            {
                WriteObject(url.ToUri());
            }
            else
            {
                WriteObject(url);
            }
        }
    }
}