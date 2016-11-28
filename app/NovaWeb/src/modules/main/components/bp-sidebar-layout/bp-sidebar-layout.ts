import {Enums} from "../../models";
import {UtilityPanelService} from "../../../shell/bp-utility-panel/bp-utility-panel";

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
            togglePanel: "&?"
        };
        this.transclude = {
            "content-left": "bpSidebarLayoutContentLeft",
            "content-center": "bpSidebarLayoutContentCenter",
            "content-right": "bpSidebarLayoutContentRight"
        };
    }
}

export class BpSidebarLayoutCtrl implements ISidebarController {

    static $inject: [string] = ["$scope", "$element", "utilityPanelService"];
    public isLeftToggled: boolean;
    public isRightToggled: boolean;

    public leftPanelTitle: string;
    public rightPanelTitle: string;

    constructor(private $scope, private $element, private utilityPanelService: UtilityPanelService) {
        this.isLeftToggled = false;
        this.isRightToggled = false;
        this.utilityPanelService.openRightSidebar = this.openRightSidebar;
    }

    public togglePanel: Function;

    public openRightSidebar = () => {
        if (!this.isRightToggled) {
            this.togglePanel({id: Enums.ILayoutPanel.Right});
            this.$scope.$apply();
        }
    };

    public toggleLeft(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.togglePanel({id: Enums.ILayoutPanel.Left});
    }

    public toggleRight(evt: ng.IAngularEvent) {
        evt.preventDefault();

        this.togglePanel({id: Enums.ILayoutPanel.Right});
    }
}
