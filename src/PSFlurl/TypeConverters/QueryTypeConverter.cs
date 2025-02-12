using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Flurl;
using PSFlurl.Extensions;

namespace PSFlurl.TypeConverters {
    public class QueryTypeConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string) ||
                sourceType == typeof(Url) ||
                sourceType.Name.StartsWith("ValueTuple`2") ||
                sourceType.Name.StartsWith("Tuple`2") ||
                typeof(IDictionary).IsAssignableFrom(sourceType) ||
                typeof(NameValueCollection).IsAssignableFrom(sourceType)
            ) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            var qpc = new QueryParamCollection();

            switch (value) {
                case string str:
                    return new QueryParamCollection(str);

                case IDictionary dict:
                    qpc.AddRange(dict.Cast<DictionaryEntry>()
                        .Select(e => new KeyValuePair<string, object>(e.Key.ToString(), e.Value)),
                        NullValueHandling.Ignore);
                    return qpc;

                case NameValueCollection nvc:
                    qpc.AddRange(nvc.AllKeys.SelectMany(key =>
                        nvc.GetValues(key).Select(val =>
                            new KeyValuePair<string, object>(key, val))),
                        NullValueHandling.Ignore);
                    return qpc;

                case Url url:
                    return url.QueryParams;

                case object obj when obj.GetType().Name.StartsWith("ValueTuple`2") ||
                                   obj.GetType().Name.StartsWith("Tuple`2"):
                    Type tupleType = obj.GetType();
                    string item1 = tupleType.GetField("Item1")?.GetValue(obj)?.ToString() ??
                                 tupleType.GetProperty("Item1")?.GetValue(obj)?.ToString();
                    object item2 = tupleType.GetField("Item2")?.GetValue(obj) ??
                                 tupleType.GetProperty("Item2")?.GetValue(obj);
                    if (item1 != null) {
                        qpc.AddRange(new[] { new KeyValuePair<string, object>(item1, item2) },
                            NullValueHandling.Ignore);
                    }
                    return qpc;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value is QueryParamCollection qpc && destinationType == typeof(string)) {
                return qpc.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}