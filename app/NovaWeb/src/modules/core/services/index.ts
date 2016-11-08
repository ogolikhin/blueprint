import {IWindowResize, WindowResize} from "./window-resize";
import {IWindowVisibility, WindowVisibility} from "./window-visibility";
import {IUserOrGroupInfo, IUsersAndGroupsService, UsersAndGroupsService} from "./users-and-groups.svc";

angular.module("bp.core")
    .service("windowResize", WindowResize)
    .service("windowVisibility", WindowVisibility);

export {
    IWindowResize,
    IWindowVisibility,
    IUserOrGroupInfo,
    IUsersAndGroupsService,
    UsersAndGroupsService
}
