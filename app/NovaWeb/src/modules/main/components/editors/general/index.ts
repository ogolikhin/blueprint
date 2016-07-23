﻿import "angular";
import {BpGeneralEditor} from "./bp-general-editor";
import {BpArtifactEditor} from "./bp-artifact-editor";

angular.module("bp.editors.details", []) 
    .component("bpGeneralEditor", new BpGeneralEditor())
    .component("bpArtifactEditor", new BpArtifactEditor());
