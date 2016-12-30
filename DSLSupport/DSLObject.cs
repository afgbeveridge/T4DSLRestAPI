using System.Collections.Generic;
using System.Linq;

namespace DSLSupport {

    public class DSLObject {

        public Dictionary<string, ExpansionSpec> RawExpansions { get; set; } = new Dictionary<string, ExpansionSpec>();

        public IEnumerable<string> OrderedExpansions => RawExpansions.Keys.OrderBy(s => s);

        public string Tag { get; set; }

        public string Singular { get; set; }

        public string Model { get; set; }

        public DSLQueryDetails QueryDetails { get; set; } = new DSLQueryDetails();

        public string ResultClass => "Composite" + Model;

        public List<Implementation> Implementations { get; set; } = new List<Implementation>();

        public bool ApiAware => QueryDetails.ApiAware;

        internal IEnumerable<string> Issues() {
            return
                new IssueRecorder()
                .Record(string.IsNullOrWhiteSpace(Tag), "Invalid tag specified")
                .Record(string.IsNullOrWhiteSpace(QueryDetails.Query) && string.IsNullOrWhiteSpace(QueryDetails.OrderProperty), "Order property must be specified when no base query, def " + Tag)
                .Issues;
        }

        internal DSLObject ApplyDefaultsIfNecessary() {
            Singular = Singular ?? Tag.Substring(0, Tag.Length - 1);
            Model = Model ?? Singular;
            return this;
        }

        internal ExpansionSpec CurrentExpansion { get; set; }

        internal ExpansionSpec AddExpansion(string name) {
            CurrentExpansion = new ExpansionSpec();
            RawExpansions[name] = CurrentExpansion;
            return CurrentExpansion;
        }

        public class Implementation {
            public string Query { get; set; }
            public string Name { get; set; }
        }

    }

}
