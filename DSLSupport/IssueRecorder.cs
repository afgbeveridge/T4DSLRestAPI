using System.Collections.Generic;
using System.Linq;

namespace DSLSupport {

    internal class IssueRecorder {

        internal List<string> Issues { get; private set; } = new List<string>();

        internal IssueRecorder(IEnumerable<string> known = null) {
            Issues.AddRange(known ?? Enumerable.Empty<string>());
        }

        internal IssueRecorder Record(bool applies, string msg) {
            if (applies)
                Issues.Add(msg);
            return this;
        }

    }

}
