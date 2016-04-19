interface ISidebarController {
    isLeftToggled: boolean;
    isRightToggled: boolean;
    type: string;
    toggleLeft(evt: ng.IAngularEvent): void;
    toggleRight(evt: ng.IAngularEvent): void;
}

export class BpSidebarLayout implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public bindings: any;
    public transclude;

    constructor() {
        this.template = require("./bp-sidebar-layout.html");
        this.controller = BpSidebarLayoutCtrl;
        this.bindings = {
            type: "@"
        };
        this.transclude = {
            "header-left" : "bpSidebarLayoutHeaderLeft",
            "content-left": "bpSidebarLayoutContentLeft",
            "content-center": "bpSidebarLayoutContentCenter",
            "header-right": "bpSidebarLayoutHeaderRight",
            "content-right": "bpSidebarLayoutContentRight"
        }
    }
}

class BpSidebarLayoutCtrl implements ISidebarController {
    static $inject: [string] = ["$scope", "$element"];
    public isLeftToggled: boolean;
    public isRightToggled: boolean;
    public type: string;

    constructor(private $scope, private $element) {
        this.isLeftToggled = true;
        this.isRightToggled = true;
        if (!this.type) {
            this.type = "bpSidebarLayout";
        }
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