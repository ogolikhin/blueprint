import * as angular from "angular";
import {BpArtifactPicker, IArtifactPickerOptions} from "./bp-artifact-picker";
import {ArtifactPickerDialogController} from "./bp-artifact-picker-dialog";

angular.module("bp.components.artifactpicker", [])
    .component("bpArtifactPicker", new BpArtifactPicker());

export {ArtifactPickerDialogController, IArtifactPickerOptions}
