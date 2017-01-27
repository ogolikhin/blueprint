import "angular";
import {BpArtifactGeneralEditorComponent} from "./artifactGeneralEditor.component";

export const GeneralEditor = angular.module("generalEditor", [])
    .component("bpArtifactGeneralEditor", new BpArtifactGeneralEditorComponent())
    .name;
