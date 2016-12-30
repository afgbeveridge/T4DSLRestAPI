using System;

namespace DSLSupport {

    internal class DSLKeyword {

        internal string Name { get; set; }

        internal bool MultipleInstance { get; set; }

        internal Func<DSLObject, DSLAmbientContext, string, IAccumulator> Handle { get; set; }

    }

}
