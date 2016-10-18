import * as angular from "angular";
import {NavigationService} from "./navigation.svc";
import {ItemInfoService} from "./item-info.svc";

angular.module("bp.core.navigation", ["ui.router"])
    .service("navigationService", NavigationService)
    .service("itemInfoService", ItemInfoService);

export {
    INavigationState
} from "./navigation-state";

export {
    INavigationService
} from "./navigation.svc";
