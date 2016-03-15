//import "./sidebar.scss"

export class Sidebar implements ng.IComponentOptions {
    public template: string = require("./sidebar.html");

    public controller: Function = SidebarCtrl;
}

class SidebarCtrl {


    public leftToggled: boolean;
    public toggleLeft(evt: ng.IAngularEvent)
    {
        evt.preventDefault();
        this.leftToggled = !this.leftToggled;
    }

    public rightToggled: boolean;
    public toggleRight(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.rightToggled = !this.rightToggled;
    }
}