export class FontNormalizer {

    private static replacer = /(font-family: )'(.+?)'/g;

    public static subsitution = {
        "Arial": "Arial, Helvetica Neue, Helvetica, sans-serif",
        "Calibri": "Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif",
        "Courier New": "Courier New, Courier, Lucida Sans Typewriter, Lucida Typewriter, monospace",
        "Cambria": "Cambria, Georgia, serif",
        "Portable User Interface": "Lucida Grande, Lucida Sans Unicode, Lucida Sans, Geneva, Verdana, sans-serif",
        "Times New Roman": "TimesNewRoman, Times New Roman, Times, Baskerville, Georgia, serif",
        "Trebuchet MS": "Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif",
        "Verdana": "Verdana, Geneva, sans-serif"
    };

    public static normalize(html: string) {
        return html ? html.replace(FontNormalizer.replacer, function (match, part1, part2) {
            let replacement = FontNormalizer.subsitution[part2];
            return replacement ? part1 + replacement + ";" : match;
        }) : html;
    }
}
