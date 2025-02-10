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

                case object obj when obj.GetType().Name.StartsWith("ValueTuple`2") || obj.GetType().Name.StartsWith("Tuple`2"):
                    Type tupleType = obj.GetType();
                    string item1 = tupleType.GetField("Item1")?.GetValue(obj)?.ToString() ??
                                  tupleType.GetProperty("Item1")?.GetValue(obj)?.ToString();
                    object item2 = tupleType.GetField("Item2")?.GetValue(obj) ??
                                  tupleType.GetProperty("Item2")?.GetValue(obj);
                    if (item1 != null) {
                        qcpFromQuery.AddRange(new[] { new KeyValuePair<string, object>(item1, item2) }, NullValueHandling.Ignore);
                    }
                    break;

                case IEnumerable<object> enumObj when enumObj.All(x =>
                    x.GetType().Name.StartsWith("ValueTuple`2") || x.GetType().Name.StartsWith("Tuple`2")):
                    IEnumerable<KeyValuePair<string, object>> kvps = enumObj.Select(tuple => {
                        Type enumTupleType = tuple.GetType();
                        string enumItem1 = enumTupleType.GetField("Item1")?.GetValue(tuple)?.ToString() ??
                                       enumTupleType.GetProperty("Item1")?.GetValue(tuple)?.ToString();
                        object enumItem2 = enumTupleType.GetField("Item2")?.GetValue(tuple) ??
                                       enumTupleType.GetProperty("Item2")?.GetValue(tuple);
                        return new KeyValuePair<string, object>(enumItem1, enumItem2);
                    }).Where(kvp => kvp.Key != null);
                    qcpFromQuery.AddRange(kvps, NullValueHandling.Ignore);
                    break;

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

                case Url url:
                    qcpFromQuery = url.QueryParams;
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