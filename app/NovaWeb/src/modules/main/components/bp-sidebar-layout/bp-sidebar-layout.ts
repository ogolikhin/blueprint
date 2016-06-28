export interface ISidebarController {
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
            bpRef: "=?",

        };
        this.transclude = {
            "content-left": "bpSidebarLayoutContentLeft",
            "content-center": "bpSidebarLayoutContentCenter",
            "content-right": "bpSidebarLayoutContentRight"
        };
    }
}

export class BpSidebarLayoutCtrl implements ISidebarController {
    public bpRef: BpSidebarLayoutCtrl;
    static $inject: [string] = ["$scope", "$element"];
    private  _isLeftToggled: boolean;
    public get isLeftToggled(): boolean {
        return this._isLeftToggled;
    }
    public set isLeftToggled(value){
        this._isLeftToggled = !!value;
    }
    private _isRightToggled: boolean;
    public get isRightToggled(): boolean {
        return this._isRightToggled;
    }
    public set isRightToggled(value) {
        this._isRightToggled = !!value;
    }

    public leftPanelTitle: string;
    public rightPanelTitle: string;

    constructor(private $scope, private $element) {
        this.bpRef = this;
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