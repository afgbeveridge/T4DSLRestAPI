using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;

namespace DSLSupport {

    public class JsonFormatDSLParser : IDSLParser {

        public string[] DSLContent { get; private set; }

        public List<DSLObject> AllDefinitions { get; private set; }

        public DSLAmbientContext AmbientContext { get; private set; }

        public bool AcceptFile(string name) {
            var content = File.ReadAllText(name);
            DSLContent = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var obj = JsonConvert.DeserializeObject<DSLGenerationContext>(string.Join(string.Empty, content));
            AllDefinitions = obj.AllDefinitions;
            AmbientContext = obj.Context;
            HandleMultiLineFormatting();
            return AllDefinitions != null;
        }

        private void HandleMultiLineFormatting() {
            const string replaceTarget = "\r\n";
            Func<string, string> format = s => string.IsNullOrEmpty(s) ? s : s.Replace(replaceTarget, Environment.NewLine);
            AmbientContext.DefaultQuery = format(AmbientContext.DefaultQuery);
            AllDefinitions
                .ForEach(def => {
                    def.QueryDetails.Query = format(def.QueryDetails.Query);
                });
        }

    }

}
