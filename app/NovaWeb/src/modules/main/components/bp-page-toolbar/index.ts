import * as angular from "angular";
import {BPPageToolbar} from "./bp-page-toolbar";

angular.module("bp.components.pagetoolbar", [])
    .component("bpPageToolbar", new BPPageToolbar());
