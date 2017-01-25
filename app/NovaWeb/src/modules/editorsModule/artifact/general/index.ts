import "angular";
import {BpArtifactGeneralEditorComponent} from "./artifactGeneralEditor.controller";

export const GeneralEditor = angular.module("generalEditor", [])
    .component("bpArtifactGeneralEditor", new BpArtifactGeneralEditorComponent())
    .name;
