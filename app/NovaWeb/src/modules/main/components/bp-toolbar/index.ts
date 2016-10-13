import * as angular from "angular";
import {BPToolbar} from "./bp-toolbar";

angular.module("bp.components.toolbar", [])
    .component("bpToolbar", new BPToolbar());
