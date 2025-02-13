using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using Flurl;
using PSFlurl.Extensions;

/// <summary>
/// Transforms generic array/IEnumerable Query input to a <see cref="QueryParamCollection"/>.
/// </summary>
namespace PSFlurl.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class FluentQueryTransformAttribute : ArgumentTransformationAttribute {

        private static readonly TypeConverter _queryConverter = TypeDescriptor.GetConverter(typeof(QueryParamCollection));

        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData) {
            return TransformQuery(inputData);
        }

        private static object TransformQuery(object query) {
            // Unwrap PSObject, if applicable
            if (query is PSObject psObject) {
                query = psObject.BaseObject;
            }

            var qcpFromQuery = new QueryParamCollection();
            switch (query) {
                case object[] array when array.Length > 0 && array[0] is string:
                    qcpFromQuery = new QueryParamCollection(string.Join("&", array));
                    break;

                case object[] array when array.Length > 0 && array[0] is IDictionary:
                    array.Cast<IDictionary>()
                        .SelectMany(dict => ((QueryParamCollection)_queryConverter.ConvertFrom(dict)))
                        .ToList()
                        .ForEach(param => qcpFromQuery.Add(param.Name, param.Value));
                    break;

                case IEnumerable<object> enumObj when enumObj.All(x =>
                    x.GetType().Name.StartsWith("ValueTuple`2") || x.GetType().Name.StartsWith("Tuple`2")):
                    enumObj
                        .SelectMany(tuple => ((QueryParamCollection)_queryConverter.ConvertFrom(tuple)))
                        .ToList()
                        .ForEach(param => qcpFromQuery.Add(param.Name, param.Value));
                    break;

                case IEnumerable<KeyValuePair<string, object>> kvp:
                    qcpFromQuery.AddRange(kvp, NullValueHandling.Ignore);
                    break;
                    
                default:
                    // Return unhandled types as-is
                    return query;
            }
            return qcpFromQuery;
        }
    }
}