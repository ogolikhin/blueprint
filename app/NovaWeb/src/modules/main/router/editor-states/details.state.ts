﻿export class ArtifactDetailsState implements ng.ui.IState {
    public template = '<bp-artifact-editor></bp-artifact-editor>';
    public params = { context: null };
    public controller = "detailsStateController";
    public controllerAs = "$content";
}

export class DetailsStateController {
    public static $inject = ["$state"];
    public context;
    constructor(
        private $state) {
        this.context = $state.params["context"];
    }
}