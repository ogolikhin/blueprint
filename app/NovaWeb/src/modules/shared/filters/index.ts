import * as angular from "angular";
import {BpFilesizeFilter} from "./bp-filesize/bp-filesize.filter";
import {BpEscapeAndHighlightFilter} from "./bp-escape-highlight";
import {BpFormat} from "./bp-format/bp-format.filter";

angular.module("bp.filters", [])
    .filter("bpFilesize", BpFilesizeFilter.factory())
    .filter("bpEscapeAndHighlight", BpEscapeAndHighlightFilter.factory())
    .filter("bpFormat", BpFormat.factory());

