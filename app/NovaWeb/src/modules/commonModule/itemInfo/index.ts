//3rd party(external) library dependencies used for this module
import "angular";
import "angular-ui-router";
//internal dependencies used for this module
import {ItemInfoService} from "./itemInfo.service";
export const ItemInfo = angular.module("itemInfo", ["ui.router"])
    .service("itemInfoService", ItemInfoService)
    .name;
//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {IItemInfoService, IItemInfoResult} from "./itemInfo.service"
