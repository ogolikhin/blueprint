import {NavigationService} from "./navigation.svc";
import {ItemInfoService} from "./item-info.svc";

angular.module("bp.core.navigation", ["ui.router"])
    .service("navigationService", NavigationService)
    .service("itemInfoService", ItemInfoService);
