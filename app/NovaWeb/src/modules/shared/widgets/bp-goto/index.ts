require("./bp-goto.scss");
import * as angular from "angular";
import {BPGotoComponent} from "./bp-goto";
import {Navigation}  from "../../../core/navigation";
angular.module("bp.widgets.goto", ["bp.widgets.filtered-input", Navigation])
    .component("bpGoto", new BPGotoComponent());
