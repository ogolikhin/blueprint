//import "./sidebar.scss"



export class PageContent implements ng.IComponentOptions {
    public template: string = require("./pagecontent.html");

    public controller: Function = PageContentCtrl; 
}

class PageContentCtrl {

    constructor() {
    }

}