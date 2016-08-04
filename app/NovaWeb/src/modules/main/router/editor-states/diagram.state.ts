﻿export class DiagramState implements ng.ui.IState {
    public template = "<bp-diagram context='$content.context'></bp-diagram>";
    public params = { context: null };
    public controller = "diagramStateController";
    public controllerAs = "$content";
}

export class DiagramStateController {
    public static $inject = ["$state"];
    public context;
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}