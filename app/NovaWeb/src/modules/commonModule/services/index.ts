//3rd party(external) library dependencies used for this module
import "angular";
import "rx/dist/rx.lite.js";
//internal dependencies used for this module
import {IWindowResize, WindowResize} from "./windowResize";
import {IWindowVisibility, WindowVisibility} from "./windowVisibility";
import {UsersAndGroupsService} from "./usersAndGroups.service";

export const CoreServices = angular.module("coreServices", [])
    .service("windowResize", WindowResize)
    .service("windowVisibility", WindowVisibility)
    .service("usersAndGroupsService", UsersAndGroupsService)
    .name;
//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {IWindowResize} from "./windowResize";
export {IWindowVisibility} from "./windowVisibility";
export {IUserOrGroupInfo, IUsersAndGroupsService, IHttpError} from "./usersAndGroups.service";
