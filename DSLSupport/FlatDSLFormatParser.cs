using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DSLSupport {

    public class FlatDSLFormatParser : IDSLParser {

        private static Regex KeywordPattern => new Regex(@"^(?<keyword>[a-zA-Z\-]+)=(?<value>.*)$");
        private const string Comment = "#";
        private const string Tag = "tag";

        private static IAccumulator NonAccumulating(Action f) {
            f();
            return null;
        }

        private static IEnumerable<DSLKeyword> Keywords = new[] {
            new DSLKeyword { Name = Tag, Handle = (o, c, s) => NonAccumulating(() => o.Tag = s) },
            new DSLKeyword { Name = "singular-tag", Handle = (o, c, s) => NonAccumulating(() => o.Singular = s) },
            new DSLKeyword { Name = "model", Handle = (o, c, s) => NonAccumulating(() => o.Model = s) },
            new DSLKeyword { Name = "setName", Handle = (o, c, s) => NonAccumulating(() => o.QueryDetails.SetName = s) },
            new DSLKeyword { Name = "orderProperty", Handle = (o, c, s) => NonAccumulating(() => o.QueryDetails.OrderProperty = s) },
            new DSLKeyword { Name = "restResourceIdProperty", Handle = (o, c, s) => NonAccumulating(() => o.QueryDetails.RestResourceIdProperty = s) },
            new DSLKeyword { Name = "restResourceIdPropertyType", Handle = (o, c, s) => NonAccumulating(() => o.QueryDetails.RestResourceIdPropertyType = s) },
            new DSLKeyword { Name = "baseQuery", 
                             Handle = (o, c, s) => new FreeFlowAccumulator(t => o.QueryDetails.Query = t) },
            new DSLKeyword { Name = "expansion", MultipleInstance = true,
                             Handle = (o, c, s) => {
                                 var spec = o.AddExpansion(s);
                                 return new ExpansionAccumulator(spec);
                             }
            },
            new DSLKeyword { Name = "defaultQuery", Handle = (o, c, s) => new FreeFlowAccumulator(t => c.DefaultQuery = t) }
        };

        private StreamReader Source { get; set; }

        public List<DSLObject> AllDefinitions { get; private set; } = new List<DSLObject>();

        public DSLAmbientContext AmbientContext { get; private set; } = new DSLAmbientContext();


        public bool AcceptFile(string name) {
            DSLContent = File.ReadAllLines(name);
            Parse();
            return true;
        }

        public string[] DSLContent { get; private set; }

        private IAccumulator CurrentAccumulator { get; set; }

        private void Parse() {
            DSLContent
                .ToList()
                    .ForEach(s => {
                        if (s.Any() && !s.StartsWith(Comment)) {
                            var match = KeywordPattern.Match(s);
                            if (!match.Success)
                                CurrentAccumulator.Add(s);
                            else
                                HandleKeyword(match, s);
                        }
                    });
        }

        private void HandleKeyword(Match match, string current) {
            var word = match.Groups["keyword"].Value;
            var val = match.Groups["value"].Value;
            if (word == Tag) {
                Current = new DSLObject();
                AllDefinitions.Add(Current);
            }
            ProcessKeyword(word, val);
        }

        private void ProcessKeyword(string word, string val) {
            CurrentAccumulator = Keywords.First(k => k.Name == word).Handle(Current, AmbientContext, val);
        }

        private DSLObject Current { get; set; }

    }

}
