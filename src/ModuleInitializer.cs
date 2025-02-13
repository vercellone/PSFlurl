using System.ComponentModel;
using System.Management.Automation;
using Flurl;
using PSFlurl.TypeConverters;

namespace PSFlurl {
    public class ModuleInitializer : IModuleAssemblyInitializer {
        public void OnImport() {
            var queryConverterAttribute = new TypeConverterAttribute(typeof(QueryTypeConverter));
            var urlConverterAttribute = new TypeConverterAttribute(typeof(UrlTypeConverter));
            TypeDescriptor.AddAttributes(typeof(QueryParamCollection), queryConverterAttribute);
            TypeDescriptor.AddAttributes(typeof(Url), urlConverterAttribute);
        }
    }
}