import "angular";

export class ErrorState implements ng.ui.IState {
    public url = "/error";
    public template = require("./error-page.html");
}
