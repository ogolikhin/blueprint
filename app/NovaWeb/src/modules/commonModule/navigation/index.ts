//3rd party(external) library dependencies used for this module
import "angular";
import "lodash";
import "angular-ui-router";
//internal dependencies used for this module
import {NavigationService} from "./navigation.service";

export const Navigation = angular.module("navigation", ["ui.router"])
    .service("navigationService", NavigationService)
    .name;
//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {
    INavigationPathItem,
    INavigationState,
    INavigationService,
    INavigationParams
} from "./navigation.service"
