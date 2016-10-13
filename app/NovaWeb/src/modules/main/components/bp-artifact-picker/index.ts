import * as angular from "angular";
import {BpArtifactPicker, ArtifactPickerDialogController, IArtifactPickerOptions} from "./bp-artifact-picker";

angular.module("bp.components.artifactpicker", [])
    .component("bpArtifactPicker", new BpArtifactPicker());

export {ArtifactPickerDialogController, IArtifactPickerOptions}
