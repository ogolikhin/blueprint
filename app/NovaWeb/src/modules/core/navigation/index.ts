import * as angular from "angular";
import {NavigationService} from "./navigation.svc";

angular.module("bp.core.navigation", ["ui.router"])
    .service("navigationService", NavigationService);

export {
    INavigationState
} from "./navigation-state";

export {
    INavigationService
} from "./navigation.svc";
