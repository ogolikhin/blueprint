import "angular";
import {BpArtifactDetailsEditor} from "./artifactDetailsEditor.controller";

export const DetailEditor = angular.module("editor.artifact.details", ["bp.widgets.dialog"])
    .component("bpArtifactDetailsEditor", new BpArtifactDetailsEditor())
    .name;
