import * as angular from "angular";
import {BpArtifactGeneralEditor} from "./artifactGeneralEditor.controller";


export const GeneralEditor = angular.module("editor.artifact.general", ["bp.widgets.dialog"])
    .component("bpArtifactGeneralEditor", new BpArtifactGeneralEditor())
    .name;
