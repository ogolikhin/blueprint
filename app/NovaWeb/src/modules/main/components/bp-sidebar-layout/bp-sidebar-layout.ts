import {Enums} from "../../models";

export interface ISidebarController {
    isLeftToggled: boolean;
    isRightToggled: boolean;
    leftPanelTitle: string;
    rightPanelTitle: string;
    togglePanel: Function;
}

export class BpSidebarLayout implements ng.IComponentOptions {
    public template: string;
    public controller: ng.Injectable<ng.IControllerConstructor>;
    public bindings: any;
    public transclude: any;

    constructor() {
        this.template = require("./bp-sidebar-layout.html");
        this.controller = BpSidebarLayoutCtrl;
        this.bindings = {
            isLeftToggled: "<",
            isRightToggled: "<",
            leftPanelTitle: "@",
            rightPanelTitle: "@",
            togglePanel: "&?",
        };
        this.transclude = {
            "content-left": "bpSidebarLayoutContentLeft",
            "content-center": "bpSidebarLayoutContentCenter",
            "content-right": "bpSidebarLayoutContentRight"
        };
    }
}

export class BpSidebarLayoutCtrl implements ISidebarController {

    static $inject: [string] = ["$scope", "$element"];
    public  isLeftToggled: boolean;
    public  isRightToggled: boolean;

    public leftPanelTitle: string;
    public rightPanelTitle: string;

    constructor(private $scope, private $element) {
        this.isLeftToggled = false;
        this.isRightToggled = false;
    }
    public $onInit() {
    }

    public $onChanged(obj: any) {
    }

    public togglePanel: Function;

    public toggleLeft(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.togglePanel({ id: Enums.ILayoutPanel.Left });
    }

    public toggleRight(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.togglePanel({ id: Enums.ILayoutPanel.Right });
    }


}