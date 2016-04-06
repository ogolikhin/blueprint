//import "./sidebar.scss"



interface ISidebarController {
    location: string;
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
            visible: "=",
            alignment: "@"
        };
    }
}

class SidebarCtrl implements ISidebarController{

    public location: string;
    private loc: string;
    public isToggled: boolean;
    constructor() {
        this.isToggled = false;
    }

    public toggle(evt: ng.IAngularEvent)
    {
        evt.preventDefault();
        this.isToggled = !this.isToggled;
    }
}