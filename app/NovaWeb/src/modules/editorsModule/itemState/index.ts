import {ItemStateService} from "./itemState.service";

export const ItemState = angular.module("itemState", [])
    .service("itemStateService", ItemStateService)
    .name;