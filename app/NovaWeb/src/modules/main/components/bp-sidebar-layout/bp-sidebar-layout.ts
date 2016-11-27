import {Enums} from "../../models";
import {BpAccordionPanelService} from "../bp-accordion/bp-accordion";

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

    static $inject: [string] = ["$scope", "$element", "bpAccordionPanelService"];
    public isLeftToggled: boolean;
    public isRightToggled: boolean;

    public leftPanelTitle: string;
    public rightPanelTitle: string;

    constructor(private $scope, private $element, private bpAccordionPanelService: BpAccordionPanelService) {
        this.isLeftToggled = false;
        this.isRightToggled = false;
        this.bpAccordionPanelService.openRightPanel = this.openRightPanel;
    }

    public togglePanel: Function;

    public openRightPanel = () => {
        if (!this.isRightToggled) {
            this.toggleRight(null);
            this.$scope.$apply();
        }
    };

    public toggleLeft(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.togglePanel({id: Enums.ILayoutPanel.Left});
    }

    public toggleRight(evt: ng.IAngularEvent) {
        if (evt) {
            evt.preventDefault();
        }

        console.log("begin");
        console.log(this.isRightToggled);
        this.togglePanel({id: Enums.ILayoutPanel.Right});
        console.log(this.isRightToggled);
        console.log("end");
    }
}
