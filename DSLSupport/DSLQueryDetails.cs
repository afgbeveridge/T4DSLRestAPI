
namespace DSLSupport {

    public class DSLQueryDetails {

        public string Query { get; set; }

        public string SetName { get; set; }

        public string OrderProperty { get; set; }

        public string RestResourceIdProperty { get; set; }

        public string RestResourceIdPropertyType { get; set; }

        public bool ApiAware => !string.IsNullOrEmpty(RestResourceIdProperty) && !string.IsNullOrEmpty(RestResourceIdPropertyType);

    }

}
