using System.Collections.Generic;
using Flurl;
using System.Linq;

namespace Flurl.Cmdlets.Utilities {
    public static class QueryParamCollectionConverter {
        public static IEnumerable<KeyValuePair<string, object>> ConvertToKeyValuePairs(QueryParamCollection collection) {
            return from kvp in collection
                   select new KeyValuePair<string, object>(kvp.Name, kvp.Value);
        }
    }
}