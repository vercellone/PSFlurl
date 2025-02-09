using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management.Automation;
using Flurl;

namespace Flurl.Cmdlets.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class FluentQueryTransformAttribute : ArgumentTransformationAttribute {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData) {
            return TransformQuery(inputData);
        }

        private static object TransformQuery(object query) {
            if (query is object[] stringArray && stringArray.Length > 0 && stringArray[0] is string) {
                query = string.Join("&", stringArray); // Bit of a hack; The Cmdlets rebuild params from scratch to honor NullValueHandling
            }

            if (query is string queryString) {
                 // The Cmdlets rebuild params from scratch to honor NullValueHandling
                return new QueryParamCollection(queryString);
            }

            if (query is IEnumerable<KeyValuePair<string, object>> kvpEnumerable) {
                return kvpEnumerable;
            }

            if (query is IDictionary dictionary) {
                return dictionary.Cast<DictionaryEntry>().Select(entry => new KeyValuePair<string, object>(entry.Key.ToString(), entry.Value));
            }

            if (query is NameValueCollection nameValueCollection) {
                return nameValueCollection.AllKeys.SelectMany(key => nameValueCollection.GetValues(key)
                    .Select(value => new KeyValuePair<string, object>(key, value)));
            }
            if (query is object[] array && array.Length > 0 && array[0] is IDictionary) {
                return array.Cast<IDictionary>().SelectMany(dict => dict.Cast<DictionaryEntry>()
                    .Select(entry => new KeyValuePair<string, object>(entry.Key.ToString(), entry.Value)));
            }
            throw new ArgumentException("Query must be a string, IDictionary, NameValueCollection, array of IDictionary, or IEnumerable<KeyValuePair<string, object>>");
        }

    }
}