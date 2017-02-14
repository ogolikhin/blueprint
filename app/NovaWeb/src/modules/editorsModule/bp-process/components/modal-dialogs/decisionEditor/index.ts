import {DecisionEditor} from "./decisionEditor.component";

export const DecisionEditorModule = angular.module("decisionEditor", [])
    .component("decisionEditor", new DecisionEditor())
    .name;
