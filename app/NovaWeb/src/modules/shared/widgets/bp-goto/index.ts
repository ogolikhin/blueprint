require("./bp-goto.scss");

import * as angular from "angular";
import { BPGotoComponent } from "./bp-goto";

angular.module("bp.widgets.goto", ["bp.widgets.filtered-input", "bp.core"])
    .component("bpGoto", new BPGotoComponent());
