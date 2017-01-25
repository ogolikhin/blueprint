// from http://stackoverflow.com/questions/17660947/can-you-override-specific-templates-in-angularui-bootstrap/26339919

// require("./uibModalWindow.html");

uibModalWindowConfig.$inject = ["$provide"];
export function uibModalWindowConfig($provide: ng.auto.IProvideService): void {
    function delegated($delegate) {
        const directive = $delegate[0];

        //directive.template = require("./uibModalWindow.html");
        directive.templateUrl = "/novaweb/static/uibModalWindow.html";

        return $delegate;
    }

    $provide.decorator("uibModalWindowDirective", ["$delegate", delegated]);
}
