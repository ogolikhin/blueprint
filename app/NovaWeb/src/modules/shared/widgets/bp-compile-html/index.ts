import * as angular from "angular";
import { BPCompileHtml } from "./bp-compile-html";

angular.module("bp.widgets.compilehtml", [])
    .directive("bpCompileHtml", BPCompileHtml.factory());

