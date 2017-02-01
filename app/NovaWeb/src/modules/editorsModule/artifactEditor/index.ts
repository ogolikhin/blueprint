import "angular";

import {DetailEditor} from "./details";
import {GeneralEditor} from "./general";

export const ArtifactEditors = angular.module("editor.artifact", [
    "bp.widgets.dialog",
    DetailEditor,
    GeneralEditor
]).name;

