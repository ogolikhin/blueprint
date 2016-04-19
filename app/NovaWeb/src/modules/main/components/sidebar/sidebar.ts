interface ISidebarController {
    isLeftToggled: boolean;
    isRightToggled: boolean;
    type: string;
    toggleLeft(evt: ng.IAngularEvent): void;
    toggleRight(evt: ng.IAngularEvent): void;
}

export class BpSidebar implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public bindings: any;
    public transclude;

    constructor() {
        this.template = require("./sidebar.html");
        this.controller = SidebarCtrl;
        this.bindings = {
            type: "@"
        };
        this.transclude = {
            "header-left" : "bpSidebarHeaderLeft",
            "content-left" : "bpSidebarContentLeft",
            "content-center" : "bpSidebarContentCenter",
            "header-right" : "bpSidebarHeaderRight",
            "content-right" : "bpSidebarContentRight"
        }
    }
}

class SidebarCtrl implements ISidebarController {
    static $inject: [string] = ["$scope", "$element"];
    public isLeftToggled: boolean;
    public isRightToggled: boolean;
    public type: string;

    constructor(private $scope, private $element) {
        this.isLeftToggled = true;
        this.isRightToggled = true;
        if (!this.type) {
            this.type = "sidebar";
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