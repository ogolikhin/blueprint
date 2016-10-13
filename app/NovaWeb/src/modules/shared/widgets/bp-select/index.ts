import * as angular from "angular";
import {BPSelect} from "./bp-select";

angular.module("bp.widgets.select", [])
    .component("bpSelect", new BPSelect());

