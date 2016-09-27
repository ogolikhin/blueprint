import * as angular from "angular";
import { BPToggleComponent } from "./bp-toggle";

angular.module("bp.widjets.toggle", [])
    .component("bpToggle", new BPToggleComponent());

