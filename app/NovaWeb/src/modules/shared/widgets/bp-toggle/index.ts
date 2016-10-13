import * as angular from "angular";
import {BPToggleComponent} from "./bp-toggle";

angular.module("bp.widgets.toggle", [])
    .component("bpToggle", new BPToggleComponent());

