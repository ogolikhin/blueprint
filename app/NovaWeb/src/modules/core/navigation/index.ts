import {NavigationService} from "./navigation.svc";

angular.module("bp.core.navigation", ["ui.router"])
        .service("navigationService", NavigationService);

export {
    INavigationState
} from "./navigation-state";

export {
    ForwardNavigationOptions,
    BackNavigationOptions
} from "./navigation-options";

export {
    INavigationService
} from "./navigation.svc";