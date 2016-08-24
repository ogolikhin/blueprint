import "angular";
import { bpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
import { bpEscapeAndHighlightFilter } from "./bp-escape-hightlight/bp-escape-highlight.filter";

angular.module("bp.filters", [])
    .filter("bpFilesize", bpFilesizeFilter.Factory())
    .filter("bpEscapeAndHighlight", bpEscapeAndHighlightFilter.Factory());

export { bpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
export { bpEscapeAndHighlightFilter } from "./bp-escape-hightlight/bp-escape-highlight.filter";
