export class BpEscapeAndHighlightFilter {

    public static $inject = [];

    public static factory() {
        let entityMap = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            "\"": "&quot;",
            "'": "&#39;",
            "/": "&#x2F;"
        };

        let escapeHtmlEntities = (toSanitize) => {
            return (toSanitize || "").replace(/[&<>"'\/]/g, s => entityMap[s]); // Escape HTML entities
        };

        let filter = () => {
            return (toFilter, toHighlight) => {
                toFilter = toFilter || "";
                toHighlight = toHighlight || "";
                let toFilterLCase = toFilter.toLowerCase();
                let toHighlightLCase = toHighlight.toLowerCase();
                let substrings = toFilterLCase.split(toHighlightLCase);
                toFilterLCase = "";
                if (substrings.length > 1 && toHighlight !== "") {
                    let pos = 0;
                    substrings.forEach((substring, index) => {
                        toFilterLCase += escapeHtmlEntities(toFilter.substr(pos, substring.length));
                        pos += substring.length;
                        if (index < substrings.length - 1) {
                            toFilterLCase += `<span class="bp-escape-highlight">` + escapeHtmlEntities(toFilter.substr(pos, toHighlight.length)) + `</span>`;
                            pos += toHighlight.length;
                        }
                    });
                    toFilter = toFilterLCase;
                } else {
                    toFilter = escapeHtmlEntities(toFilter);
                }

                return toFilter;
            };
        };

        filter.$inject = [];

        return filter;
    }
}
