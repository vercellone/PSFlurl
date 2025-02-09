using System;
using System.Collections.Generic;
using System.Management.Automation;
using Flurl;
using PSFlurl.Attributes;
using PSFlurl.Extensions;
using PSFlurl.Utilities;

namespace PSFlurl.Cmdlets {
    [Cmdlet(VerbsCommon.New, "FlQuery")]
    [OutputType(typeof(string))]
    [OutputType(typeof(QueryParamCollection))]
    public class NewFlQueryCommand : PSCmdlet {

        /// <summary>
        /// <para type="description">The query parameters.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [FluentQueryTransform()]
        public object Query { get; set; }

        /// <summary>
        /// <para type="description">Specifies how to handle null values in the query parameters.</para>
        /// </summary>
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public NullValueHandling NullValueHandling { get; set; } = NullValueHandling.Remove;

        /// <summary>
        /// <para type="description">Specifies whether to output the query parameters as a string.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter AsString { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to output the query parameters as a string with spaces as plus signs.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter EncodeSpaceAsPlus { get; set; }

        protected override void ProcessRecord() {
            QueryParamCollection fluentQuery = new QueryParamCollection();

            if (Query is QueryParamCollection collection) {
                IEnumerable<KeyValuePair<string, object>> kvpEnumerable = QueryParamCollectionConverter.ConvertToKeyValuePairs(collection);
                fluentQuery.AddRange(kvpEnumerable, this.NullValueHandling);
            }
            else if (Query is IEnumerable<KeyValuePair<string, object>> kvpEnumerable) {
                fluentQuery.AddRange(kvpEnumerable, this.NullValueHandling);
            }
            else {
                WriteError(new ErrorRecord(new ArgumentException("Query must be a string, IDictionary, NameValueCollection, array of IDictionary, or IEnumerable<KeyValuePair<string, object>>"), "InvalidArgument", ErrorCategory.InvalidArgument, Query));
                return;
            }

            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent) {
                WriteObject(fluentQuery.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else {
                WriteObject(fluentQuery);
            }
        }
    }
}