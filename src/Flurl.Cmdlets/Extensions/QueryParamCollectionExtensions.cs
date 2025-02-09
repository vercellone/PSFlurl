using System.Collections.Generic;

namespace Flurl.Cmdlets.Extensions {
    public static class QueryParamCollectionExtensions {
        public static void AddRange(this QueryParamCollection fluentQuery, IEnumerable<KeyValuePair<string, object>> kvpEnumerable, NullValueHandling nullValueHandling) {
            foreach (KeyValuePair<string, object> kvp in kvpEnumerable) {
                object value = kvp.Value == null || string.IsNullOrWhiteSpace($"{kvp.Value}") ? null : kvp.Value;
                if (nullValueHandling == NullValueHandling.Ignore && value == null) {
                    value = string.Empty;
                }
                fluentQuery.Add(kvp.Key, value, false, nullValueHandling);
            }
        }
    }
}