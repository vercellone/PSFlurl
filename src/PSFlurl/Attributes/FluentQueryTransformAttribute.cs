using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management.Automation;
using Flurl;
using PSFlurl.Extensions;

namespace PSFlurl.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class FluentQueryTransformAttribute : ArgumentTransformationAttribute {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData) {
            return TransformQuery(inputData);
        }

        private static object TransformQuery(object query) {
            // Unwrap PSObject, if applicable
            if (query is PSObject psObject) {
                query = psObject.BaseObject;
            }

            // Return QueryParamCollection as-is
            if (query is QueryParamCollection qpc) {
                return qpc;
            }

            // Convert known types to QueryParamCollection
            var qcpFromQuery = new QueryParamCollection();
            switch (query) {
                case string queryString:
                    qcpFromQuery = new QueryParamCollection(queryString);
                    break;

                case object[] array when array.Length > 0 && array[0] is string:
                    qcpFromQuery = new QueryParamCollection(string.Join("&", array));
                    break;

                case object[] array when array.Length > 0 && array[0] is IDictionary:
                    foreach (IDictionary dict in array.Cast<IDictionary>()) {
                        qcpFromQuery.AddRange(dict.Cast<DictionaryEntry>()
                            .Select(e => new KeyValuePair<string, object>(e.Key.ToString(), e.Value)), NullValueHandling.Ignore);
                    }
                    break;

                case object obj when obj.GetType().Name.StartsWith("ValueTuple`2"):
                    throw new ArgumentTransformationMetadataException(
                        "QueryParamCollection was enumerated. To pipe a stored QueryParamCollection, use (prepend) the comma operator: ,$query | New-Flurl"
                    );

                case IEnumerable<KeyValuePair<string, object>> kvp:
                    qcpFromQuery.AddRange(kvp, NullValueHandling.Ignore);
                    break;

                case IDictionary dict:
                    qcpFromQuery.AddRange(dict.Cast<DictionaryEntry>()
                        .Select(e => new KeyValuePair<string, object>(e.Key.ToString(), e.Value)), NullValueHandling.Ignore);
                    break;

                case NameValueCollection nvc:
                    qcpFromQuery.AddRange(nvc.AllKeys.SelectMany(key =>
                        nvc.GetValues(key).Select(value =>
                            new KeyValuePair<string, object>(key, value))), NullValueHandling.Ignore);
                    break;

                default:
                    Console.WriteLine("Unhandled type: " + query.GetType().FullName);
                    // Return unhandled types as-is
                    return query;
            }
            return qcpFromQuery;
        }
    }
}