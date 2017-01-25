import "angular";
import {BpArtifactDetailsEditor} from "./artifactDetailsEditor.controller";

export const DetailEditor = angular.module("detailsEditor", ["bp.widgets.dialog"])
    .component("bpArtifactDetailsEditor", new BpArtifactDetailsEditor())
    .name;
