interface ISidebarController {
    isToggled: boolean;
    label: string;
    type: string;
    toggle(evt: ng.IAngularEvent): void
}

export class Sidebar implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public bindings: any;
    public transclude: boolean = true;

    constructor() {
        this.template = require("./sidebar.html");
        this.controller = SidebarCtrl;
        this.bindings = {
            type: "@"
        };
    }
}

class SidebarCtrl implements ISidebarController {
    static $inject: [string] = ["$scope", "$element"];
    public isToggled: boolean;
    public label: string;
    public type: string;

    constructor(private $scope, private $element) {
        this.isToggled = false;
        if(!this.type) this.type = 'sidebar';

        switch(this.type) {
            case 'utility-panel':
                this.label = 'Utility Panel';
                break;
            case 'project-explorer':
                this.label = 'Project Explorer';
                break;
            default:
                this.label = 'Sidebar';
        }
    }

    public toggle(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.isToggled = !this.isToggled;
        if(this.isToggled) {
            this.$element.addClass('show');
            this.$scope.$parent.main.$element.addClass(this.type +'-visible');
        } else {
            this.$element.removeClass('show');
            this.$scope.$parent.main.$element.removeClass(this.type +'-visible');
        }
    }
}