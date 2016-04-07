//import "./sidebar.scss"



interface ISidebarController {
    isToggled: boolean;
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
        };
    }
}

class SidebarCtrl implements ISidebarController {
    static $inject: [string] = ["$element"];
    public isToggled: boolean;

    constructor(private $element) {
        this.isToggled = false;
    }

    public toggle(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.isToggled = !this.isToggled;
        if(this.isToggled) this.$element.addClass('show');
        else this.$element.removeClass('show');
    }
}