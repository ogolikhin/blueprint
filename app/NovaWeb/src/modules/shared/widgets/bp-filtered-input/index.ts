import * as angular from "angular";

import { BPFilteredInput } from "./bp-filtered-input";

angular.module("bp.widgets.filtered-input", [])
    .directive("bpFilteredInput", BPFilteredInput.factory());
