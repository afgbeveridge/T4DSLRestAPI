using System.Collections.Generic;

namespace DSLSupport {

    public interface IDSLParser {

        string[] DSLContent { get; }

        bool AcceptFile(string name);

        List<DSLObject> AllDefinitions { get; }

        DSLAmbientContext AmbientContext { get; }

    }
}