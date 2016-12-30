
namespace DSLSupport {

    public class ExpansionSpec {

        public string Type { get; set; }

        public string Join { get; set; }

        public string Predicate { get; set; }

        internal void Accept(Semantics sem, string val) {
            if (sem == Semantics.Type)
                Type = val;
            else if (sem == Semantics.Join)
                Join = val;
            else
                Predicate = val;
        }

        internal enum Semantics { Type = 1, Join = 2, Predicate = 4 };
    }

}
