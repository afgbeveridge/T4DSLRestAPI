using System;
using System.Linq;

namespace DSLSupport {

    internal interface IAccumulator {
        void Add(string src);
    }

    internal class ExpansionAccumulator : IAccumulator {

        private ExpansionSpec Spec { get; set; }

        private int Stage { get; set; } = 1;

        internal ExpansionAccumulator(ExpansionSpec spec) {
            Spec = spec;
        }

        public void Add(string src) {
            Spec.Accept((ExpansionSpec.Semantics)Stage, src);
            Stage = Stage << 1;
        }

    }

    internal class FreeFlowAccumulator : IAccumulator {

        private string Content { get; set; } = string.Empty;

        private Action<string> Target { get; set; }

        internal FreeFlowAccumulator(Action<string> target) {
            Target = target;
        }

        public void Add(string src) {
            Content += (Content.Any() ? Environment.NewLine : string.Empty) + src;
            Target(Content);
        }
    }


}
