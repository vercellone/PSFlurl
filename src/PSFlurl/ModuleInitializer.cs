using System.ComponentModel;
using System.Management.Automation;
using Flurl;
using PSFlurl.TypeConverters;

namespace PSFlurl {
    public class ModuleInitializer : IModuleAssemblyInitializer {
        public void OnImport() {
            var typeConverter = new UrlTypeConverter();
            var attribute = new TypeConverterAttribute(typeof(UrlTypeConverter));
            TypeDescriptor.AddAttributes(typeof(Url), attribute);
        }
    }
}