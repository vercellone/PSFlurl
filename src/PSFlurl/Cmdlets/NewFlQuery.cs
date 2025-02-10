using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Flurl;
using PSFlurl.Attributes;
using PSFlurl.Extensions;
using PSFlurl.Utilities;

namespace PSFlurl.Cmdlets {
    [Cmdlet(VerbsCommon.New, "FlQuery")]
    [Alias("Get-FlQuery")]
    [OutputType(typeof(string))]
    [OutputType(typeof(QueryParamCollection))]
    public class NewFlQueryCommand : PSCmdlet {

        private QueryParamCollection _queryParams = new QueryParamCollection();

        /// <summary>
        /// <para type="description">The query parameters.</para>
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
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
            if (Query != null) {
                if (Query is QueryParamCollection collection) {
                    IEnumerable<KeyValuePair<string, object>> kvpEnumerable = QueryParamCollectionConverter.ConvertToKeyValuePairs(collection);
                    _queryParams.AddRange(kvpEnumerable, NullValueHandling);
                }
                else if (Query is IEnumerable<KeyValuePair<string, object>> kvpEnumerable) {
                    _queryParams.AddRange(kvpEnumerable, NullValueHandling);
                }
                else if (Query.GetType().Name.StartsWith("ValueTuple`2") || Query.GetType().Name.StartsWith("Tuple`2")) {
                    Type tupleType = Query.GetType();
                    string item1 = tupleType.GetField("Item1")?.GetValue(Query)?.ToString();
                    object item2 = tupleType.GetField("Item2")?.GetValue(Query);
                    if (item1 != null) {
                        _queryParams.Add(item1, item2, false, NullValueHandling);
                    }
                }
                else {
                    WriteError(new ErrorRecord(
                        new ArgumentException($"Query ({Query.GetType().FullName}) must be string(s), IDictionary(s), Tuples(s), NameValueCollection, QueryParamCollection, or IEnumerable<KeyValuePair<string, object>>"),
                        "InvalidArgument",
                        ErrorCategory.InvalidArgument,
                        Query));
                }
            }

            // Only output immediately if we're not using the pipeline
            if (!MyInvocation.BoundParameters.ContainsKey(nameof(Query)) ||
                !GetType().GetProperty(nameof(Query)).GetCustomAttributes(typeof(ParameterAttribute), true)
                    .Cast<ParameterAttribute>()
                    .Any(a => a.ValueFromPipeline)) {
                OutputResult();
            }
        }

        protected override void EndProcessing() {
            // Only params accumulated from the pipeline
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Query)) &&
                GetType().GetProperty(nameof(Query)).GetCustomAttributes(typeof(ParameterAttribute), true)
                    .Cast<ParameterAttribute>()
                    .Any(a => a.ValueFromPipeline)) {
                OutputResult();
            }
            base.EndProcessing();
        }

        private void OutputResult() {
            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent) {
                WriteObject(_queryParams.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else {
                WriteObject(_queryParams, false);
            }
        }
    }
}