using System;
using System.ComponentModel;
using System.Globalization;
using Flurl;

namespace PSFlurl.TypeConverters {
    public class UrlTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string) || sourceType == typeof(Uri))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(string) || destinationType == typeof(Uri))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string str)
                return new Url(str);
            if (value is Uri uri)
                return new Url(uri.ToString());
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value is Url url) {
                if (destinationType == typeof(string))
                    return url.ToString();
                if (destinationType == typeof(Uri))
                    return new Uri(url.ToString());
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}