interface ISidebarController {
    isLeftToggled: boolean;
    isRightToggled: boolean;
    toggleLeft(evt: ng.IAngularEvent): void;
    toggleRight(evt: ng.IAngularEvent): void;

    leftPanelTitle: string;
    rightPanelTitle: string;
}

export class BpSidebarLayout implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public bindings: any;
    public transclude: any;

    constructor() {
        this.template = require("./bp-sidebar-layout.html");
        this.controller = BpSidebarLayoutCtrl;
        this.bindings = {
            leftPanelTitle: "@",
            rightPanelTitle: "@",
        };
        this.transclude = {
            "content-left": "bpSidebarLayoutContentLeft",
            "content-center": "bpSidebarLayoutContentCenter",
            "content-right": "bpSidebarLayoutContentRight"
        };
    }
}

class BpSidebarLayoutCtrl implements ISidebarController {
    static $inject: [string] = ["$scope", "$element"];
    public isLeftToggled: boolean;
    public isRightToggled: boolean;

    public leftPanelTitle: string;
    public rightPanelTitle: string;

    constructor(private $scope, private $element) {
        this.isLeftToggled = true;
        this.isRightToggled = true;
    }

    public toggleLeft(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.isLeftToggled = !this.isLeftToggled;
    }

    public toggleRight(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.isRightToggled = !this.isRightToggled;
    }
}