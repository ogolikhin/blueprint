import "angular";
import { BpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
import { BpEscapeAndHighlightFilter } from "./bp-escape-hightlight/bp-escape-highlight.filter";

angular.module("bp.filters", [])
    .filter("BpFilesize", BpFilesizeFilter.Factory())
    .filter("BpEscapeAndHighlight", BpEscapeAndHighlightFilter.Factory());

export { BpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
export { BpEscapeAndHighlightFilter } from "./bp-escape-hightlight/bp-escape-highlight.filter";
