export class ErrorComponent implements ng.IComponentOptions {
    public template: string = require("./error.html");
    public controller: Function = ErrorController;
    public bindings: any = {
        message: "@"
    };
}

export class ErrorController {
    public static $inject = ["$sce"];
    public message: string;
    constructor(private $sce: any) {        
    }

    public getText() {
        return this.$sce.trustAsHtml(this.message);
    }
}
