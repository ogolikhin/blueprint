import * as angular from "angular";
import { BpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
import { BpEscapeAndHighlightFilter } from "./bp-escape-hightlight/bp-escape-highlight.filter";

angular.module("bp.filters", [])
    .filter("bpFilesize", BpFilesizeFilter.factory())
    .filter("bpEscapeAndHighlight", BpEscapeAndHighlightFilter.factory());

export { BpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
export { BpEscapeAndHighlightFilter } from "./bp-escape-hightlight/bp-escape-highlight.filter";
