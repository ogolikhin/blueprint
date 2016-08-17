﻿import "angular";


import { BpArtifactGeneralEditor } from "./bp-general-editor";
import { BpArtifactDetailsEditor } from "./bp-details-editor";

angular.module("bp.editors.details", [])
    .component("bpArtifactGeneralEditor", new BpArtifactGeneralEditor())
    .component("bpArtifactDetailsEditor", new BpArtifactDetailsEditor());


