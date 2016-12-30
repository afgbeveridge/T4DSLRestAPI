using System;
using System.Collections.Generic;
using System.Linq;

namespace DSLSupport {

    public class DSLProcessor {

        private const string AnonymousStart = "a.";
        private const string FirstSelectedObject = "obj";
        private const string padding = "                ";
        public string NameSeparator { get; set; } = "_";
        public string MethodNameStart { get; set; } = "Get";

        private IDSLParser Parser { get; set; } = new FlatDSLFormatParser();

        public DSLProcessor(IDSLParser parser = null, string nameSeparator = null, string methodNameStart = null) {
            Parser = parser ?? Parser;
            NameSeparator = nameSeparator ?? NameSeparator;
            MethodNameStart = methodNameStart ?? MethodNameStart;
        }

        public DSLProcessor UseFile(string name) {
            Parser.AcceptFile(name);
            SelfCheck();
            AllDefinitions
                .ForEach(def => {
                    def.ApplyDefaultsIfNecessary();
                    // Fill in query if needed
                    if (string.IsNullOrWhiteSpace(def.QueryDetails.Query)) {
                        var propertySub = AnonymousStart + def.Singular + "." + def.QueryDetails.OrderProperty;
                        def.QueryDetails.Query = Parser.AmbientContext.DefaultQuery
                                                    .Replace(DSLAmbientContext.CollectionNameToken, def.QueryDetails.SetName ?? def.Tag)
                                                    .Replace(DSLAmbientContext.OrderPropertyToken, propertySub)
                                                    .Replace(DSLAmbientContext.BaseEntityToken, def.Singular + " = " + FirstSelectedObject);
                    }
                    ProcessObject(def);
                });
            return this;
        }

        public List<DSLObject> AllDefinitions => Parser.AllDefinitions;

        public string[] DSLContent => Parser.DSLContent;

        private void SelfCheck() {
            var issues = new IssueRecorder(AllDefinitions.SelectMany(obj => obj.Issues()))
                            .Record(AllDefinitions.Any(d => string.IsNullOrWhiteSpace(d.QueryDetails.Query)) && 
                                    string.IsNullOrWhiteSpace(Parser.AmbientContext.DefaultQuery), "No default query and at least one def requires it")
                            .Issues;
            if (issues.Any())
                throw new Exception("Invalid DSL instance " + string.Join(Environment.NewLine, issues));
        }

        // https://rosettacode.org/wiki/Power_set#C.23
        private IEnumerable<IEnumerable<string>> FormPowerSet(IList<string> names) {
            return from m in Enumerable.Range(0, 1 << names.Count)
                   select
                       from i in Enumerable.Range(0, names.Count)
                       where (m & (1 << i)) != 0
                       select names[i];
        }

        private void ProcessObject(DSLObject obj) {
            FormPowerSet(obj.RawExpansions.Keys.ToList())
                .ToList().ForEach(src => {
                    var names = src.OrderBy(s => s);
                    var joins = FormJoins(obj, names);
                    var whereClauses = string.Join(Environment.NewLine, obj.RawExpansions.Where(kvp => names.Contains(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value.Predicate))
                                    .Select(kvp => padding + kvp.Value.Predicate));
                    var joinClauses = string.Join(Environment.NewLine, joins.Select(j => padding + j));
                    var interimObjectSelect = padding + ".Select(a => " + obj.ResultClass + ".Accept(" +
                                       "a." + obj.Singular + (names.Any() ? ", " : string.Empty) +
                                        string.Join(",", names.Select(n => n.ToLower() + ": a." + n)) + "));";
                    var fullQuery = (string.Join(Environment.NewLine, obj.QueryDetails.Query.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(l => l.StartsWith("{") ? l : padding + l)) +
                                    Environment.NewLine +
                                    interimObjectSelect)
                                    .Replace("{joins}", joinClauses)
                                    .Replace("{extraWhere}", whereClauses);
                    obj.Implementations.Add(new DSLObject.Implementation {
                        Name = string.Join(NameSeparator, new[] { MethodNameStart, obj.Tag }.Concat(names)),
                        Query = fullQuery
                    }
                    );
                });
        }

        private IList<string> FormJoins(DSLObject obj, IEnumerable<string> names) {
            var cur = "a." + obj.Singular;
            List<string> joins = new List<string>();
            names
                .ToList()
                .ForEach(n => {
                    // Get join clause template
                    var template = obj.RawExpansions[n];
                    var ident = n.ToLower();
                    string selector = "(a, " + ident + ") => new { " + cur + ", " + n + " = " + ident + "}";
                    joins.Add(template.Join.Replace("{selector}", selector));
                    cur = cur + ", a." + n;
                });
            return joins;
        }

    }

}
