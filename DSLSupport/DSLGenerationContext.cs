using System.Collections.Generic;

namespace DSLSupport {

    public class DSLGenerationContext {

        public DSLAmbientContext Context { get; set; }

        public List<DSLObject> AllDefinitions { get; set; }

    }

}
