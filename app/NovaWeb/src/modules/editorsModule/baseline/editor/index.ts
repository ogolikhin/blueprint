import * as angular from "angular";

import {BpArtifactBaselineEditor} from "./baselineEditor.component";

export const BaselineEditor = angular.module("baselineEditor", [])
    .component("bpBaseline", new BpArtifactBaselineEditor()).name;
