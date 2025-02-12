using System;
using System.Management.Automation;
using Flurl;
using PSFlurl.Attributes;

namespace PSFlurl.Cmdlets {
    [Cmdlet(VerbsCommon.New, "FlQuery")]
    [Alias("Get-FlQuery")]
    [OutputType(typeof(string))]
    [OutputType(typeof(QueryParamCollection))]
    public class NewFlQueryCommand : PSCmdlet {

        private readonly QueryParamCollection _queryParams = new QueryParamCollection();

        /// <summary>
        /// <para type="description">The query parameters.</para>
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [FluentQueryTransform()]
        public QueryParamCollection Query { get; set; }

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
                foreach ((string Name, object Value) in Query) {
                    object val = Value != null && string.IsNullOrWhiteSpace($"{Value}") ? null : Value;
                    _queryParams.Add(Name, val, false, NullValueHandling);
                }
            }
        }

        protected override void EndProcessing() {
            if (AsString.IsPresent || EncodeSpaceAsPlus.IsPresent) {
                WriteObject(_queryParams.ToString(EncodeSpaceAsPlus.IsPresent));
            }
            else {
                WriteObject(_queryParams, false);
            }
        }
    }
}